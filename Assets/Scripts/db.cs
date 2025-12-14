using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class SupabaseDB : MonoBehaviour
{
    // =======================
    //  CONFIG
    // =======================
    [SerializeField] private string supabaseUrl = "https://sclrgqkxyaplnqnkcgdj.supabase.co";
    [SerializeField] private string supabaseAnonKey =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InNjbHJncWt4eWFwbG5xbmtjZ2RqIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjA0NTUyNTksImV4cCI6MjA3NjAzMTI1OX0.SotGUcP7HaDdF6z6rQtQEwZN8mpafyw2sBv5fkJHDBE";

    // ENDPOINTS (PostgREST)
    private string Jugador => $"{supabaseUrl}/rest/v1/Jugador";
    private string Partida => $"{supabaseUrl}/rest/v1/Partida";
    private string Nivel   => $"{supabaseUrl}/rest/v1/Nivel";
    private string Logro   => $"{supabaseUrl}/rest/v1/Logro";
    private string JugadorLogro => $"{supabaseUrl}/rest/v1/JugadorLogro";

    void Start()
    {
        // DEMO: probar algunos flujos
        StartCoroutine(GetJugadores());
        //StartCoroutine(CrearJugador("Samuel Hurtado", "sam@demo.com", "SamHZ"));
        //StartCoroutine(CrearPartida(1, nivelInicial: 1));
        //StartCoroutine(CerrarPartida(idPartida: 5, nivelFinal: 3, scoreFinal: 1234, segundos: 785, resultado: "victoria"));
        //StartCoroutine(GetPartidasDeJugador(1));
    }

    // =========================================
    //  LISTAR JUGADORES
    // =========================================
    public IEnumerator GetJugadores()
    {
        string url = $"{Jugador}?select=*";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            AddStdHeaders(req);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Jugadores: {req.downloadHandler.text}");
            else
                Debug.LogError($"❌ GetJugadores: {req.error}\n{req.downloadHandler.text}");
        }
    }

    // =========================================
    //  CREAR JUGADOR
    //  Campos: nJIdJugador (IDENTITY), cJNombre, cJEmail, cJNickName, dJFechaRegistro (default)
    // =========================================
    public IEnumerator CrearJugador(string nombre, string email, string nickname)
    {
        string bodyJson = $"{{\"cJNombre\":\"{Escape(nombre)}\",\"cJEmail\":\"{Escape(email)}\",\"cJNickName\":\"{Escape(nickname)}\"}}";
        using (UnityWebRequest req = BuildPost(Jugador, bodyJson))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Jugador creado: {req.downloadHandler.text}");
            else
                Debug.LogError($"❌ CrearJugador: {req.error}\n{req.downloadHandler.text}");
        }
    }

    // =========================================
    //  CREAR PARTIDA (inicio)
    //  Campos clave: nPIdJugador, dPFechaInicio (now), nPNivelInicial
    //  nPScore y nPTiempo quedan en 0 y se actualizan al cerrar
    // =========================================
    public IEnumerator CrearPartida(int idJugador, int? nivelInicial = null)
    {
        string nowIso = DateTime.UtcNow.ToString("o"); // ISO 8601
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.AppendFormat("\"nPIdJugador\":{0},", idJugador);
        sb.AppendFormat("\"dPFechaInicio\":\"{0}\"", nowIso);
        if (nivelInicial.HasValue) sb.AppendFormat(",\"nPNivelInicial\":{0}", nivelInicial.Value);
        sb.Append("}");

        using (UnityWebRequest req = BuildPost(Partida, sb.ToString()))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Partida creada: {req.downloadHandler.text}");
            else
                Debug.LogError($"❌ CrearPartida: {req.error}\n{req.downloadHandler.text}");
        }
    }

    // =========================================
    //  CERRAR PARTIDA (update por PK)
    //  Actualiza dPFechaFin, nPNivelFinal, nPScore, nPTiempo, cPResultado
    //  PostgREST PATCH con filtro ?nPIdPartida=eq.<id>
    // =========================================
    public IEnumerator CerrarPartida(int idPartida, int nivelFinal, int scoreFinal, int segundos, string resultado /* 'victoria'|'derrota'|'abandono' */)
    {
        string nowIso = DateTime.UtcNow.ToString("o");
        string bodyJson = $"{{\"dPFechaFin\":\"{nowIso}\",\"nPNivelFinal\":{nivelFinal},\"nPScore\":{scoreFinal},\"nPTiempo\":{segundos},\"cPResultado\":\"{Escape(resultado)}\"}}";
        string url = $"{Partida}?nPIdPartida=eq.{idPartida}";

        using (UnityWebRequest req = BuildPatch(url, bodyJson))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Partida cerrada: {req.downloadHandler.text}");
            else
                Debug.LogError($"❌ CerrarPartida: {req.error}\n{req.downloadHandler.text}");
        }
    }

    // =========================================
    //  LISTAR PARTIDAS DE UN JUGADOR (orden por fecha)
    // =========================================
    public IEnumerator GetPartidasDeJugador(int idJugador, int limit = 20)
    {
        string url = $"{Partida}?select=*&nPIdJugador=eq.{idJugador}&order=dPFechaInicio.desc&limit={limit}";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            AddStdHeaders(req);
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log($"✅ Partidas de jugador {idJugador}: {req.downloadHandler.text}");
            else
                Debug.LogError($"❌ GetPartidasDeJugador: {req.error}\n{req.downloadHandler.text}");
        }
    }

    // =========================================
    //  UTILIDADES
    // =========================================
    private void AddStdHeaders(UnityWebRequest req)
    {
        req.SetRequestHeader("apikey", supabaseAnonKey);
        req.SetRequestHeader("Authorization", "Bearer " + supabaseAnonKey);
        req.SetRequestHeader("Accept", "application/json");
    }

    private UnityWebRequest BuildPost(string url, string json)
    {
        var req = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddStdHeaders(req);
        req.SetRequestHeader("Prefer", "return=representation"); // devuelve el registro creado
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    private UnityWebRequest BuildPatch(string url, string json)
    {
        var req = new UnityWebRequest(url, "PATCH");
        byte[] body = Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        AddStdHeaders(req);
        req.SetRequestHeader("Prefer", "return=representation");
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    private string Escape(string s) => s?.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
