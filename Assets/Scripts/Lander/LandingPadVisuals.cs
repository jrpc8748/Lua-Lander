using TMPro;
using UnityEngine;

public class LandingPadVisuals : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreMultiplierTextMesh;

    private void Awake()
    {
        LandingPad landingPad = GetComponent<LandingPad>();
        scoreMultiplierTextMesh.text = "x" + landingPad.GetScoreMultiplier();
    }
}
