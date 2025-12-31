using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Support
{
    public static class SceneExtensions
    {
        public static async UniTask LoadAdditiveSceneAsync(string sceneName, IProgress<float> progress = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[LoadAdditiveSceneAsync] Scene name is null or empty.");
                return;
            }

            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"[LoadAdditiveSceneAsync] Failed to load scene: {sceneName}");
                return;
            }

            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                progress?.Report(op.progress / 0.9f);
                await UniTask.Yield();
            }

            progress?.Report(1f);
            op.allowSceneActivation = true;

            await UniTask.WaitUntil(() => op.isDone);

            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                SceneManager.SetActiveScene(scene);
            }
            else
            {
                Debug.LogError($"[LoadAdditiveSceneAsync] Scene not valid after load: {sceneName}");
            }
        }

        public static async UniTask UnloadSceneIfLoaded(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[UnloadSceneIfLoaded] Scene name is null or empty.");
                return;
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(scene);
            }
            else
            {
                Debug.LogWarning($"[UnloadSceneIfLoaded] Scene '{sceneName}' is not loaded or invalid.");
            }
        }

        public static IObservable<Unit> LoadScene(string sceneName)
        {
            return Observable.Create<Unit>(subject =>
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError("[LoadScene] Scene name is null or empty.");
                    subject.OnCompleted();
                    return Disposable.Empty;
                }

                var disposable = new CompositeDisposable();

                //TODO disable for reload scene
                //var activeScene = SceneManager.GetActiveScene().name;
                // if (activeScene == sceneName)
                // {
                //     Debug.Log($"[LoadScene] Scene '{sceneName}' is already active.");
                //     subject.OnNext(Unit.Default);
                //     subject.OnCompleted();
                //     return Disposable.Empty;
                // }

                void Handler(AsyncOperation operation)
                {
                    if (operation != null)
                    {
                        operation.completed -= Handler;
                        subject.OnNext(Unit.Default);
                        subject.OnCompleted();
                    }
                }

                var operation = SceneManager.LoadSceneAsync(sceneName);
                if (operation == null)
                {
                    Debug.LogError($"[LoadScene] Failed to start loading scene: {sceneName}");
                    subject.OnCompleted();
                    return Disposable.Empty;
                }

                operation.completed += Handler;

                Disposable
                    .Create(() => operation.completed -= Handler)
                    .AddTo(disposable);

                return disposable;
            });
        }

        public static T LoadSceneRoot<T>() where T : MonoBehaviour
        {
            var result = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (result == null)
            {
                Debug.LogError($"[LoadSceneRoot<{typeof(T).Name}>] Root object not found in scene.");
            }

            return result;
        }
    }
}