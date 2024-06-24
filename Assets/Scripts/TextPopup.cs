using UnityEngine;
using TMPro;

public class TextPopup : MonoBehaviour
{

    private static GameObject pfTextPopup;
    public static void SetTextPopupPrefab(GameObject prefab) {
        pfTextPopup = prefab;
    }

    public static TextPopup Create(Vector3 position, string text, Color color, float size) {
        var damagePopupGameObject = Instantiate(pfTextPopup, position, Quaternion.identity);

        TextPopup damagePopup = damagePopupGameObject.GetComponent<TextPopup>();
        
        damagePopup.transform.rotation = Quaternion.LookRotation(damagePopup.transform.position - Camera.main.transform.position);
        
        damagePopup.Setup(text, color, size);

        return damagePopup;
    }

    private static int sortingOrder;

    private const float DISAPPEAR_TIMER_MAX = 2f;

    private TextMeshPro textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake() {
        textMesh = transform.GetComponent<TextMeshPro>();
    }

    public void Setup(string text, Color color, float size) {
        textMesh.SetText(text);

        textMesh.color = color;
        textColor = color;
        disappearTimer = DISAPPEAR_TIMER_MAX;
        
        textMesh.fontSize = size;

        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;

        moveVector = new Vector3(0.5f, 0.05f);
    }

    private void Update() {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * .5f) {
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * (increaseScaleAmount * Time.deltaTime);
        } else {
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * (decreaseScaleAmount * Time.deltaTime);
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0) {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if (textColor.a < 0) {
                Destroy(gameObject);
            }
        }
    }

}
