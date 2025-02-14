// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Game.Skinning.Editor;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;
using static osu.Game.Tests.Visual.Navigation.TestSceneScreenNavigation;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneSkinEditorSceneLibrary : OsuGameTestScene
    {
        private SkinEditor skinEditor;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            Screens.Select.SongSelect songSelect = null;
            PushAndConfirm(() => songSelect = new TestPlaySongSelect());
            AddUntilStep("wait for song select", () => songSelect.BeatmapSetsLoaded);

            AddStep("import beatmap", () => BeatmapImportHelper.LoadQuickOszIntoOsu(Game).WaitSafely());

            AddUntilStep("wait for selected", () => !Game.Beatmap.IsDefault);

            AddStep("open skin editor", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.S);
                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            AddUntilStep("get skin editor", () => (skinEditor = Game.ChildrenOfType<SkinEditor>().FirstOrDefault()) != null);
        }

        [Test]
        public void TestEditComponentDuringGameplay()
        {
            switchToGameplayScene();

            BarHitErrorMeter hitErrorMeter = null;

            AddUntilStep("select bar hit error blueprint", () =>
            {
                var blueprint = skinEditor.ChildrenOfType<SkinBlueprint>().FirstOrDefault(b => b.Item is BarHitErrorMeter);

                if (blueprint == null)
                    return false;

                hitErrorMeter = (BarHitErrorMeter)blueprint.Item;
                skinEditor.SelectedComponents.Clear();
                skinEditor.SelectedComponents.Add(blueprint.Item);
                return true;
            });

            AddAssert("value is default", () => hitErrorMeter.JudgementLineThickness.IsDefault);

            AddStep("hover first slider", () =>
            {
                InputManager.MoveMouseTo(
                    skinEditor.ChildrenOfType<SkinSettingsToolbox>().First()
                              .ChildrenOfType<SettingsSlider<float>>().First()
                              .ChildrenOfType<SliderBar<float>>().First()
                );
            });

            AddStep("adjust slider via keyboard", () => InputManager.Key(Key.Left));

            AddAssert("value is less than default", () => hitErrorMeter.JudgementLineThickness.Value < hitErrorMeter.JudgementLineThickness.Default);
        }

        [Test]
        public void TestAutoplayCompatibleModsRetainedOnEnteringGameplay()
        {
            AddStep("select DT", () => Game.SelectedMods.Value = new Mod[] { new OsuModDoubleTime() });

            switchToGameplayScene();

            AddAssert("DT still selected", () => ((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Single() is OsuModDoubleTime);
        }

        [Test]
        public void TestAutoplayIncompatibleModsRemovedOnEnteringGameplay()
        {
            AddStep("select no fail and spun out", () => Game.SelectedMods.Value = new Mod[] { new OsuModNoFail(), new OsuModSpunOut() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        [Test]
        public void TestDuplicateAutoplayModRemovedOnEnteringGameplay()
        {
            AddStep("select autoplay", () => Game.SelectedMods.Value = new Mod[] { new OsuModAutoplay() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        [Test]
        public void TestCinemaModRemovedOnEnteringGameplay()
        {
            AddStep("select cinema", () => Game.SelectedMods.Value = new Mod[] { new OsuModCinema() });

            switchToGameplayScene();

            AddAssert("no mod selected", () => !((Player)Game.ScreenStack.CurrentScreen).Mods.Value.Any());
        }

        private void switchToGameplayScene()
        {
            AddStep("Click gameplay scene button", () => skinEditor.ChildrenOfType<SkinEditorSceneLibrary.SceneButton>().First(b => b.Text == "Gameplay").TriggerClick());

            AddUntilStep("wait for player", () =>
            {
                DismissAnyNotifications();
                return Game.ScreenStack.CurrentScreen is Player;
            });
        }
    }
}
