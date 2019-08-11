﻿using BepInEx;
using Studio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KK_StudioObjectMoveHotkeys
{
    [BepInProcess("CharaStudio")]
    [BepInPlugin(GUID, PluginName, Version)]
    public class KK_StudioObjectMoveHotkeys : BaseUnityPlugin
    {
        public const string GUID = "com.deathweasel.bepinex.studioobjectmovehotkeys";
        public const string PluginName = "Studio Object Move Hotkeys";
        public const string Version = "1.0";

        private Key KeyMode = Key.NONE;
        private Vector2 BeginMousePos;
        private Vector2 LastMousePos;
        private Dictionary<int, Vector3> OldPos;
        private Dictionary<int, Vector3> OldRot;
        private Dictionary<int, Vector3> OldScale;
        internal GuideObject FirstTarget;
        private Dictionary<int, GuideObject> Targets = new Dictionary<int, GuideObject>();

        public const float MOVE_RATIO = 2.5f;
        public const float ROTATE_RATIO = 90f;
        public const float SCALE_RATIO = 0.5f;

        public static SavedKeyboardShortcut HotkeyX { get; private set; }
        public static SavedKeyboardShortcut HotkeyY { get; private set; }
        public static SavedKeyboardShortcut HotkeyZ { get; private set; }
        public static SavedKeyboardShortcut HotkeyAll { get; private set; }

        private void Main()
        {
            HotkeyX = new SavedKeyboardShortcut(nameof(HotkeyX), nameof(KK_StudioObjectMoveHotkeys), new KeyboardShortcut(KeyCode.Y));
            HotkeyY = new SavedKeyboardShortcut(nameof(HotkeyY), nameof(KK_StudioObjectMoveHotkeys), new KeyboardShortcut(KeyCode.U));
            HotkeyZ = new SavedKeyboardShortcut(nameof(HotkeyZ), nameof(KK_StudioObjectMoveHotkeys), new KeyboardShortcut(KeyCode.I));
            HotkeyAll = new SavedKeyboardShortcut(nameof(HotkeyAll), nameof(KK_StudioObjectMoveHotkeys), new KeyboardShortcut(KeyCode.T));
        }

        public GuideObject GetTargetObject() => Singleton<GuideObjectManager>.Instance.operationTarget ?? Singleton<GuideObjectManager>.Instance.selectObject;

        private void Update()
        {
            if (Singleton<GuideObjectManager>.Instance == null) return;
            GuideObjectMode guideObjectMode = (GuideObjectMode)Singleton<GuideObjectManager>.Instance.mode;

            Vector2 vector = GetMousePos() - LastMousePos;

            switch (KeyMode)
            {
                case Key.OBJ_MOVE_X:
                    Move(new Vector3(-vector.x, 0f, 0f));
                    if (!HotkeyX.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_MOVE)
                        FinishMove();
                    break;
                case Key.OBJ_MOVE_Y:
                    Move(new Vector3(0f, vector.y, 0f));
                    if (!HotkeyY.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_MOVE)
                        FinishMove();
                    break;
                case Key.OBJ_MOVE_Z:
                    Move(new Vector3(0f, 0f, -vector.y));
                    if (!HotkeyZ.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_MOVE)
                        FinishMove();
                    break;
                case Key.OBJ_ROT_X:
                    Rotate(new Vector3((vector.x + vector.y) / 2f, 0f, 0f));
                    if (!HotkeyX.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_ROT)
                        FinishRotate();
                    break;
                case Key.OBJ_ROT_Y:
                    Rotate(new Vector3(0f, (vector.x + vector.y) / 2f, 0f));
                    if (!HotkeyY.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_ROT)
                        FinishRotate();
                    break;
                case Key.OBJ_ROT_Z:
                    Rotate(new Vector3(0f, 0f, (vector.x + vector.y) / 2f));
                    if (!HotkeyZ.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_ROT)
                        FinishRotate();
                    break;
                case Key.OBJ_SCALE_X:
                    Scale(Vector3.left * ((GetMousePos() - BeginMousePos).x + (GetMousePos() - BeginMousePos).y) / 2f);
                    if (!HotkeyX.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_SCALE)
                        FinishScale();
                    break;
                case Key.OBJ_SCALE_Y:
                    Scale(Vector3.up * ((GetMousePos() - BeginMousePos).x + (GetMousePos() - BeginMousePos).y) / 2f);
                    if (!HotkeyY.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_SCALE)
                        FinishScale();
                    break;
                case Key.OBJ_SCALE_Z:
                    Scale(Vector3.forward * ((GetMousePos() - BeginMousePos).x + (GetMousePos() - BeginMousePos).y) / 2f);
                    if (!HotkeyZ.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_SCALE)
                        FinishScale();
                    break;
                case Key.OBJ_SCALE_ALL:
                    Scale(Vector3.one * ((GetMousePos() - BeginMousePos).x + (GetMousePos() - BeginMousePos).y) / 2f);
                    if (!HotkeyAll.IsPressed() || guideObjectMode != GuideObjectMode.OBJ_SCALE)
                        FinishScale();
                    break;
                case Key.NONE:
                    if (Singleton<GuideObjectManager>.Instance.selectObjects == null || Singleton<GuideObjectManager>.Instance.selectObjects.Length == 0)
                        return;

                    switch (guideObjectMode)
                    {
                        case GuideObjectMode.OBJ_MOVE:
                            if (HotkeyX.IsPressed())
                            {
                                KeyMode = Key.OBJ_MOVE_X;
                                OldPos = CollectOldPos();
                            }
                            else if (HotkeyY.IsPressed())
                            {
                                KeyMode = Key.OBJ_MOVE_Y;
                                OldPos = CollectOldPos();
                            }
                            else if (HotkeyZ.IsPressed())
                            {
                                KeyMode = Key.OBJ_MOVE_Z;
                                OldPos = CollectOldPos();
                            }
                            break;
                        case GuideObjectMode.OBJ_ROT:
                            if (HotkeyX.IsPressed())
                            {
                                KeyMode = Key.OBJ_ROT_X;
                                FirstTarget = GetTargetObject();
                                OldPos = CollectOldPos();
                                OldRot = CollectOldRot();
                            }
                            else if (HotkeyY.IsPressed())
                            {
                                KeyMode = Key.OBJ_ROT_Y;
                                FirstTarget = GetTargetObject();
                                OldPos = CollectOldPos();
                                OldRot = CollectOldRot();
                            }
                            else if (HotkeyZ.IsPressed())
                            {
                                KeyMode = Key.OBJ_ROT_Z;
                                FirstTarget = GetTargetObject();
                                OldPos = CollectOldPos();
                                OldRot = CollectOldRot();
                            }
                            break;
                        case GuideObjectMode.OBJ_SCALE:
                            if (HotkeyX.IsPressed())
                            {
                                KeyMode = Key.OBJ_SCALE_X;
                                OldScale = CollectOldScale();
                                BeginMousePos = GetMousePos();
                            }
                            else if (HotkeyY.IsPressed())
                            {
                                KeyMode = Key.OBJ_SCALE_Y;
                                OldScale = CollectOldScale();
                                BeginMousePos = GetMousePos();
                            }
                            else if (HotkeyZ.IsPressed())
                            {
                                KeyMode = Key.OBJ_SCALE_Z;
                                OldScale = CollectOldScale();
                                BeginMousePos = GetMousePos();
                            }
                            else if (HotkeyAll.IsPressed())
                            {
                                KeyMode = Key.OBJ_SCALE_ALL;
                                OldScale = CollectOldScale();
                                BeginMousePos = GetMousePos();
                            }
                            break;
                    }
                    break;
                default:
                    KeyMode = Key.NONE;
                    break;
            }
            LastMousePos = GetMousePos();
        }

        private Dictionary<int, Vector3> CollectOldPos()
        {
            Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
            Targets = new Dictionary<int, GuideObject>();
            foreach (GuideObject guideObject in Singleton<GuideObjectManager>.Instance.selectObjects)
            {
                if (guideObject.enablePos)
                {
                    dictionary.Add(guideObject.dicKey, guideObject.changeAmount.pos);
                    Targets.Add(guideObject.dicKey, guideObject);
                }
            }
            return dictionary;
        }

        private Dictionary<int, Vector3> CollectOldRot()
        {
            Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
            Targets = new Dictionary<int, GuideObject>();
            foreach (GuideObject guideObject in Singleton<GuideObjectManager>.Instance.selectObjects)
            {
                if (guideObject.enableRot)
                {
                    dictionary.Add(guideObject.dicKey, guideObject.changeAmount.rot);
                    Targets.Add(guideObject.dicKey, guideObject);
                }
            }
            return dictionary;
        }

        private Dictionary<int, Vector3> CollectOldScale()
        {
            Dictionary<int, Vector3> dictionary = new Dictionary<int, Vector3>();
            Targets = new Dictionary<int, GuideObject>();
            foreach (GuideObject guideObject in Singleton<GuideObjectManager>.Instance.selectObjects)
            {
                if (guideObject.enableScale)
                {
                    dictionary.Add(guideObject.dicKey, guideObject.changeAmount.scale);
                    Targets.Add(guideObject.dicKey, guideObject);
                }
            }
            return dictionary;
        }

        private void Move(Vector3 delta)
        {
            Camera mainCmaera = Studio.Studio.Instance.cameraCtrl.mainCmaera;
            if (mainCmaera != null)
            {
                Vector3 vector = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, Input.mousePosition.z);
                Ray ray = mainCmaera.ScreenPointToRay(vector);
                ray.direction = new Vector3(ray.direction.x, 0f, ray.direction.z);
                Vector3 vector2 = ray.direction * -1f * delta.z;
                ray.direction = Quaternion.LookRotation(ray.direction) * Vector3.right;
                vector2 += ray.direction * -1f * delta.x;
                vector2.y = delta.y;
                delta = vector2;
            }
            delta = delta * Studio.Studio.optionSystem.manipuleteSpeed * MOVE_RATIO;
            foreach (GuideObject guideObject in Targets.Values)
            {
                if (guideObject.enablePos)
                {
                    guideObject.transformTarget.position = guideObject.transformTarget.position + delta;
                    guideObject.changeAmount.pos = guideObject.transformTarget.localPosition;
                }
            }
        }

        private void FinishMove()
        {
            GuideCommand.EqualsInfo[] changeAmountInfo = (from v in Targets
                                                          select new GuideCommand.EqualsInfo
                                                          {
                                                              dicKey = v.Key,
                                                              oldValue = OldPos[v.Key],
                                                              newValue = v.Value.changeAmount.pos
                                                          }).ToArray();
            Singleton<UndoRedoManager>.Instance.Push(new GuideCommand.MoveEqualsCommand(changeAmountInfo));
            KeyMode = Key.NONE;
        }

        private void Rotate(Vector3 delta)
        {
            delta = delta * Studio.Studio.optionSystem.manipuleteSpeed * ROTATE_RATIO;
            foreach (GuideObject guideObject in Targets.Values)
            {
                if (guideObject.enableRot)
                {
                    Vector3 localEulerAngles = guideObject.transformTarget.localEulerAngles += delta;
                    guideObject.transformTarget.localEulerAngles = localEulerAngles;
                    guideObject.changeAmount.rot = guideObject.transformTarget.localEulerAngles;
                }
            }
        }

        private void FinishRotate()
        {
            GuideObjectManager instance = Singleton<GuideObjectManager>.Instance;
            GuideCommand.EqualsInfo[] changeAmountInfo = (from v in Targets
                                                          select new GuideCommand.EqualsInfo
                                                          {
                                                              dicKey = v.Key,
                                                              oldValue = OldRot[v.Key],
                                                              newValue = v.Value.changeAmount.rot
                                                          }).ToArray();
            Singleton<UndoRedoManager>.Instance.Push(new GuideCommand.RotationEqualsCommand(changeAmountInfo));
            KeyMode = Key.NONE;
        }

        private void Scale(Vector3 scaleDelta)
        {
            Vector3 vector = scaleDelta * Studio.Studio.optionSystem.manipuleteSpeed * SCALE_RATIO;
            foreach (GuideObject guideObject in Targets.Values)
            {
                if (guideObject.enableRot)
                {
                    Vector3 vector2 = OldScale[guideObject.dicKey];
                    vector2.x *= 1f + vector.x;
                    vector2.y *= 1f + vector.y;
                    vector2.z *= 1f + vector.z;
                    guideObject.transformTarget.localScale = vector2;
                    guideObject.changeAmount.scale = vector2;
                }
            }
        }

        private void FinishScale()
        {
            GuideObjectManager instance = Singleton<GuideObjectManager>.Instance;
            GuideCommand.EqualsInfo[] changeAmountInfo = (from v in Targets
                                                          select new GuideCommand.EqualsInfo
                                                          {
                                                              dicKey = v.Key,
                                                              oldValue = OldScale[v.Key],
                                                              newValue = v.Value.changeAmount.scale
                                                          }).ToArray();
            Singleton<UndoRedoManager>.Instance.Push(new GuideCommand.ScaleEqualsCommand(changeAmountInfo));
            KeyMode = Key.NONE;
        }

        private Vector2 GetMousePos()
        {
            Vector2 vector = Input.mousePosition;
            return new Vector2(vector.x / Screen.width, vector.y / Screen.height);
        }

        public enum Key
        {
            OBJ_MOVE_X,
            OBJ_MOVE_Y,
            OBJ_MOVE_Z,
            OBJ_ROT_X,
            OBJ_ROT_Y,
            OBJ_ROT_Z,
            OBJ_SCALE_X,
            OBJ_SCALE_Y,
            OBJ_SCALE_Z,
            OBJ_SCALE_ALL,
            NONE
        }
        public enum GuideObjectMode
        {
            OBJ_MOVE,
            OBJ_ROT,
            OBJ_SCALE
        }
    }
}