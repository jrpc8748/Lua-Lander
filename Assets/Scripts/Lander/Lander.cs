using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Lander: MonoBehaviour
{
    public static Lander Instance { get; private set; }

    private const float GRAVITY_NORMAL = .7F;

    public event EventHandler OnLeftForce;
    public event EventHandler OnUpForce;
    public event EventHandler OnRightForce;
    public event EventHandler OnBeforeForce;
    public event EventHandler OnCoinPickup;
    public event EventHandler OnFuelPickup;
    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs{
        public State state;
    }
    public event EventHandler<OnLandedEventArgs> OnLanded;
    public class OnLandedEventArgs : EventArgs{
        public LandingType landingType;
        public int score;
        public float dotVector;
        public float landingSpeed;
        public float scoreMultiplier;
    }

    public enum LandingType{
        Success,
        WrongLanding,
        TooSteepAngle,
        TooFastLanding,
    }

    public enum State
    {
        WaitingToStart,
        Normal,
        GameOver,
    }

    private Rigidbody2D landerRigidbody2D;
    private float fuelAmount;
    private float fuelAmountMax = 10f;
    private State state;
    void Awake(){
        Instance = this;

        fuelAmount = fuelAmountMax;
        
        state = State.WaitingToStart;

        landerRigidbody2D = GetComponent<Rigidbody2D>();
        landerRigidbody2D.gravityScale = 0f; 
    }

    // Update is called once per frame
    void FixedUpdate(){
        OnBeforeForce?.Invoke(this, EventArgs.Empty);

        switch (state){
            default:
            case State.WaitingToStart:
                  if (GameInput.Instance.IsUpActionPressed() ||
                   GameInput.Instance.IsLeftActionPressed() ||
                   GameInput.Instance.IsRightActionPressed() ||
                   GameInput.Instance.GetMovementVector2() != Vector2.zero ){
                    // Pressing any input
                    landerRigidbody2D.gravityScale = GRAVITY_NORMAL;
                    SetState(State.Normal);
             
                }
                break;
            case State.Normal:

                if (GameInput.Instance.IsUpActionPressed() ||
                   GameInput.Instance.IsLeftActionPressed() ||
                   GameInput.Instance.IsRightActionPressed() ||
                   GameInput.Instance.GetMovementVector2() != Vector2.zero)
                {
                    // Pressing any input
                    ConsumeFuel();
                }

                float gamepadDeadZone = .4f;
                // this is for input system
                if (GameInput.Instance.IsUpActionPressed() || GameInput.Instance.GetMovementVector2().y > gamepadDeadZone)
                {
                    float force = 700f;
                    landerRigidbody2D.AddForce(force * transform.up * Time.deltaTime);
                    OnUpForce?.Invoke(this, EventArgs.Empty);
                }
                if (GameInput.Instance.IsLeftActionPressed() || GameInput.Instance.GetMovementVector2().x < -gamepadDeadZone)
                {
                    float turnSpeed = +100f;
                    landerRigidbody2D.AddTorque(turnSpeed * Time.deltaTime);
                    OnLeftForce?.Invoke(this, EventArgs.Empty);
                }
                if (GameInput.Instance.IsRightActionPressed() || GameInput.Instance.GetMovementVector2().x > gamepadDeadZone)
                {
                    float turnSpeed = -100f;
                    landerRigidbody2D.AddTorque(turnSpeed * Time.deltaTime);
                    OnRightForce?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GameOver:
                break;
        }

        //Debug.Log(fuelAmount); 
        if(fuelAmount <= 0)
        {
            // No Fuel
            return;
        }


    }

    private void OnCollisionEnter2D(Collision2D collision2D)
    {
        if(!collision2D.gameObject.TryGetComponent(out LandingPad landingPad)){
            Debug.Log("Crashed on the terrain!");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.WrongLanding,
                dotVector = 0f,
                landingSpeed = 0f,
                scoreMultiplier = 0,
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }

        float softLandingVelocityMagnitude = 4f;
        float relativeVelocityMagnitude = collision2D.relativeVelocity.magnitude;
        if (relativeVelocityMagnitude > softLandingVelocityMagnitude)
        {
            // Landed too Hard!
            Debug.Log("Landed too Hard!");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooFastLanding,
                dotVector = 0f,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = 0,
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }

        float dotVector = Vector2.Dot(Vector2.up, transform.up);
        float minDotVector = .9f;
        if(dotVector < minDotVector)
        {
            // Landed on too steep angle!
            Debug.Log("Landed on too steep angle!");
            OnLanded?.Invoke(this, new OnLandedEventArgs
            {
                landingType = LandingType.TooSteepAngle,
                dotVector = dotVector,
                landingSpeed = relativeVelocityMagnitude,
                scoreMultiplier = landingPad.GetScoreMultiplier(),
                score = 0,
            });
            SetState(State.GameOver);
            return;
        }
        //Debug.Log(dotVector);
        Debug.Log("Successful Landing.");

        float maxScoreAmountLandingAngle = 100;
        float scoreDotVectorMultiplier = 10f;
        float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

        float maxScoreAmountLandingSpeed = 100;
        float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude) * maxScoreAmountLandingSpeed;

        //Debug.Log("landingAngleScore = " + landingAngleScore);
        //Debug.Log("landingSpeedScore = " + landingSpeedScore);
        //Debug.Log();

        int score = Mathf.RoundToInt((landingAngleScore + landingSpeedScore) * landingPad.GetScoreMultiplier());
        Debug.Log("Score:" + score);
        OnLanded?.Invoke(this, new OnLandedEventArgs{
            landingType = LandingType.Success,
            dotVector = dotVector,
            landingSpeed = relativeVelocityMagnitude,
            scoreMultiplier = landingPad.GetScoreMultiplier(),
            score = score,
        });
        SetState(State.GameOver);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if(collider2D.gameObject.TryGetComponent(out FuelScript fuel))
        {
            float addFuelAmount = 10f;
            fuelAmount += addFuelAmount;
            if(fuelAmount > fuelAmountMax){
                fuelAmount = fuelAmountMax;
            }
            OnFuelPickup?.Invoke(this, EventArgs.Empty);
            fuel.DestroyFuel();
        }
        if(collider2D.gameObject.TryGetComponent(out CoinScript coin))
        {
            OnCoinPickup?.Invoke(this, EventArgs.Empty);
            coin.DestroyCoin();
        }
    }

    private void SetState(State state)
    {
        this.state = state;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
        {
            state = state,
        });  
    }
    private void ConsumeFuel()
    {
        float fuelConsumptionAmount = 1f;
        fuelAmount -= fuelConsumptionAmount * Time.deltaTime;
    }

    public float GetFuel()
    {
        return fuelAmount;
    }
    public float GetfuelAmountNormalized()
    {
        return fuelAmount / fuelAmountMax;
    }
    public float GetSpeedX(){
        return landerRigidbody2D.linearVelocityX;
    }
    public float GetSpeedY(){
        return landerRigidbody2D.linearVelocityY;
    }
}
