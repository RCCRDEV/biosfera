using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Biosfera.EditorTools
{
    /// <summary>
    /// Configuração automática do projeto no Editor (ZERO-CLIQUE).
    /// Ao abrir o Unity este script cria, se necessário:
    ///   1) as pastas Assets/Scenes e Assets/Resources;
    ///   2) o AnimatorController "SEEED07_Animator" com os 5 estados
    ///      obrigatórios (Idle, Walk, Run, Jump, Fall);
    ///   3) a cena Assets/Scenes/Level1.unity;
    ///   4) a entrada da cena no Build Settings;
    ///   5) os shaders usados por código na lista "Always Included Shaders"
    ///      (evita materiais rosa em builds).
    /// Nada precisa ser feito manualmente pelo aluno.
    /// </summary>
    [InitializeOnLoad]
    public static class BiosferaEditorSetup
    {
        private const string ScenePath = "Assets/Scenes/Level1.unity";
        private const string AnimatorPath = "Assets/Resources/SEEED07_Animator.controller";
        private const string SessionKey = "BiosferaSetupExecutado";

        /// <summary>
        /// Construtor estático chamado pelo Unity a cada recarregamento de
        /// scripts. Agenda a configuração para o próximo ciclo do Editor.
        /// </summary>
        static BiosferaEditorSetup()
        {
            EditorApplication.delayCall += RunOnce;
        }

        /// <summary>
        /// Executa a configuração uma única vez por sessão do Editor.
        /// É pública para poder ser chamada também pela linha de comando
        /// (-executeMethod Biosfera.EditorTools.BiosferaEditorSetup.RunOnce).
        /// </summary>
        public static void RunOnce()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            SessionState.SetBool(SessionKey, true);

            try
            {
                EnsureFolder("Assets/Scenes");
                EnsureFolder("Assets/Resources");

                EnsureAnimatorController();
                EnsureScene();
                EnsureBuildSettings();
                EnsureAlwaysIncludedShaders();

                AssetDatabase.SaveAssets();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[BIOSFERA] Setup automático do Editor: " + e.Message);
            }
        }

        /// <summary>
        /// Cria uma pasta (e seus pais) caso ainda não exista.
        /// </summary>
        /// <param name="path">Caminho da pasta, ex.: "Assets/Scenes".</param>
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            int lastSlash = path.LastIndexOf('/');
            string parent = path.Substring(0, lastSlash);
            string leaf = path.Substring(lastSlash + 1);

            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        /// <summary>
        /// Cria o AnimatorController com os 5 estados de animação exigidos
        /// pelo trabalho (Idle, Walk, Run, Jump, Fall) e os parâmetros usados
        /// pelo PlayerController.
        /// </summary>
        private static void EnsureAnimatorController()
        {
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorPath) != null) return;

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
            if (controller == null) return;

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            string[] states = { "Idle", "Walk", "Run", "Jump", "Fall" };

            AnimatorState first = null;
            for (int i = 0; i < states.Length; i++)
            {
                AnimatorState state = machine.AddState(states[i]);
                state.writeDefaultValues = false;
                if (i == 0) first = state;
            }
            machine.defaultState = first;

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[BIOSFERA] AnimatorController criado com os 5 estados de animação.");
        }

        /// <summary>
        /// Cria a cena Level1.unity (com câmera e luz padrão) caso ela ainda
        /// não exista, e a deixa aberta para que o Play funcione de imediato.
        /// </summary>
        private static void EnsureScene()
        {
            Scene active = EditorSceneManager.GetActiveScene();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                if (string.IsNullOrEmpty(active.path) && !active.isDirty)
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }

            // A cena Level1 ainda não existe.
            // Salvaguarda: nunca descarta uma cena REAL que tenha alterações
            // não salvas.
            if (!string.IsNullOrEmpty(active.path) && active.isDirty) return;

            // Se a cena ativa é uma cena nova (sem arquivo), salvamos ELA
            // mesma como Level1 - assim nada do que está aberto é perdido.
            Scene scene = string.IsNullOrEmpty(active.path) && active.IsValid()
                ? active
                : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[BIOSFERA] Cena " + ScenePath + " criada automaticamente.");
        }

        /// <summary>
        /// Garante que a cena Level1 esteja registrada no Build Settings,
        /// colocando-a como primeira cena da lista.
        /// </summary>
        private static void EnsureBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ScenePath)
                {
                    if (i == 0 && scenes[i].enabled) return;
                    scenes.RemoveAt(i);
                    break;
                }
            }

            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[BIOSFERA] Cena adicionada ao Build Settings.");
        }

        /// <summary>
        /// Adiciona à lista "Always Included Shaders" os shaders que o jogo
        /// procura por código em tempo de execução. Sem isso, um build final
        /// poderia exibir materiais rosa (shader removido do build).
        /// </summary>
        private static void EnsureAlwaysIncludedShaders()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
            if (assets == null || assets.Length == 0) return;

            SerializedObject settings = new SerializedObject(assets[0]);
            SerializedProperty list = settings.FindProperty("m_AlwaysIncludedShaders");
            if (list == null || !list.isArray) return;

            string[] wanted = { "Standard", "Particles/Standard Unlit", "Sprites/Default", "UI/Default" };
            bool changed = false;

            for (int w = 0; w < wanted.Length; w++)
            {
                Shader shader = Shader.Find(wanted[w]);
                if (shader == null) continue;

                bool already = false;
                for (int i = 0; i < list.arraySize; i++)
                {
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                    {
                        already = true;
                        break;
                    }
                }
                if (already) continue;

                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = shader;
                changed = true;
            }

            if (changed)
            {
                settings.ApplyModifiedProperties();
                Debug.Log("[BIOSFERA] Shaders usados por código adicionados a Always Included Shaders.");
            }
        }
    }
}
