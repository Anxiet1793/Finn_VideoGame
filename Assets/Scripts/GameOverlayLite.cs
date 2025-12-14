#define TMP_PRESENT

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

// ------------ ACEPTAR CERTIFICADOS (DEMO / NO PRODUCCIÓN) ------------
class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

[Serializable]
public class WorldTimeResponse {
    public string timezone;
    public string datetime;     // ej: "2025-10-14T12:34:56.789012-05:00"
    public string utc_datetime;
    public long unixtime;
    public string utc_offset;
}

[Serializable]
public class CatFactResponse {
    public string fact;
    public int length;
}

public class GameOverlayLite : MonoBehaviour
{
    [Header("World Time API")]
    // vuelve a HTTPS; el handler se encarga del SSL
    [SerializeField] private string timeApiBase = "https://worldtimeapi.org/api/timezone";
    [SerializeField] private string timeZone = "America/Lima";
    [SerializeField] private int timeRefreshSeconds = 60;

    [Header("Cat Fact API")]
    [SerializeField] private string catFactUrl = "https://catfact.ninja/fact";

    [Header("UI de Hora (elige Text o TMP)")]
    [SerializeField] private Text uiTextClock; // UI legacy (opcional)
#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI tmpTextClock; // TMP (usa este)
#endif

    [Header("UI opcional para Cat Fact")]
    [SerializeField] private Text uiTextCatFact; // opcional
#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI tmpTextCatFact; // opcional
#endif

    public string LatestCatFact { get; private set; } = "";

    void Start()
    {
        StartCoroutine(UpdateClockLoop());
        StartCoroutine(FetchCatFact());
    }

    // =========================
    // Hora (WorldTimeAPI) con SSL handler + fallback local
    // =========================
    IEnumerator UpdateClockLoop()
    {
        while (true)
        {
            yield return FetchAndShowTime();
            yield return new WaitForSeconds(timeRefreshSeconds);
        }
    }

    IEnumerator FetchAndShowTime()
    {
        string url = $"{timeApiBase}/{UnityWebRequest.EscapeURL(timeZone)}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            // <<< CLAVE PARA EVITAR EL ERROR DE SSL EN TU EQUIPO/RED >>>
            req.certificateHandler = new AcceptAllCertificates();
            req.disposeCertificateHandlerOnDispose = true;

            req.timeout = 10;
            req.SetRequestHeader("User-Agent", "Unity-Student-Project"); // algunos servidores lo agradecen
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var json = req.downloadHandler.text;
                var data = JsonUtility.FromJson<WorldTimeResponse>(json);
                string hhmm = "--:--";

                if (data != null && !string.IsNullOrEmpty(data.datetime))
                {
                    try {
                        DateTime dt = DateTime.Parse(data.datetime);
                        hhmm = dt.ToString("HH:mm");
                    } catch {
                        int tPos = data.datetime.IndexOf('T');
                        if (tPos > 0 && tPos + 6 <= data.datetime.Length)
                            hhmm = data.datetime.Substring(tPos + 1, 5);
                    }
                }

                SetClockText(hhmm);
            }
            else
            {
                // Fallback local para que nunca se quede vacío
                string local = DateTime.Now.ToString("HH:mm");
                Debug.LogError($"❌ Hora API: {req.error} | {req.downloadHandler.text} -> usando hora local {local}");
                SetClockText(local + " (local)");
            }
        }
    }

    void SetClockText(string msg)
    {
        if (uiTextClock != null) uiTextClock.text = msg;
#if TMP_PRESENT
        if (tmpTextClock != null) tmpTextClock.text = msg;
#endif
    }

    // =========================
    // Cat Fact (catfact.ninja)
    // =========================
    IEnumerator FetchCatFact()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(catFactUrl))
        {
            // también aceptamos SSL por si tu red es quisquillosa
            req.certificateHandler = new AcceptAllCertificates();
            req.disposeCertificateHandlerOnDispose = true;

            req.timeout = 10;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var json = req.downloadHandler.text;
                var data = JsonUtility.FromJson<CatFactResponse>(json);
                if (data != null && !string.IsNullOrEmpty(data.fact))
                {
                    LatestCatFact = data.fact;
                    Debug.Log("🐱 Cat Fact: " + LatestCatFact);
                    SetCatFactText(LatestCatFact);
                }
                else
                {
                    Debug.LogError("❌ Cat Fact: respuesta inválida.");
                }
            }
            else
            {
                Debug.LogError($"❌ Cat Fact: {req.error} | {req.downloadHandler.text}");
            }
        }
    }

    void SetCatFactText(string msg)
    {
        if (uiTextCatFact != null) uiTextCatFact.text = msg;
#if TMP_PRESENT
        if (tmpTextCatFact != null) tmpTextCatFact.text = msg;
#endif
    }
}
