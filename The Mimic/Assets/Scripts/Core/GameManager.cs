using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheMimic
{
    // The only singleton: run state (Playing/Won/Dead), restart, and a bare end-of-run overlay.
    public class GameManager : MonoBehaviour
    {
        public enum RunState { Playing, Won, Dead }

        public static GameManager Instance { get; private set; }

        public RunState State { get; private set; } = RunState.Playing;
        public float TimeSurvived { get; private set; }
        public event Action<RunState> OnStateChanged;

        // DeathScreen sets this so the detailed overlay replaces the bare one below.
        public bool SuppressDefaultOverlay { get; set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("[GameManager] Duplicate GameManager destroyed. Keep exactly one in the scene.", this);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Win() => EndRun(RunState.Won);
        public void PlayerDied() => EndRun(RunState.Dead);

        void EndRun(RunState result)
        {
            if (State != RunState.Playing)
                return;

            State = result;
            TimeSurvived = Time.timeSinceLevelLoad;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log($"[GameManager] Run over: {result} after {TimeSurvived:0.0}s.", this);
            OnStateChanged?.Invoke(result);
        }

        void Update()
        {
            if (State == RunState.Playing)
                return;

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnGUI()
        {
            if (State == RunState.Playing || SuppressDefaultOverlay)
                return;

            string text = State == RunState.Won ? "YOU ESCAPED" : "YOU DIED";
            var style = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0f, Screen.height / 2f - 60f, Screen.width, 60f), text, style);

            var small = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0f, Screen.height / 2f, Screen.width, 30f), "Press R to restart", small);
        }
    }
}
