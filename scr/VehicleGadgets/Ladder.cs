namespace VehicleGadgetsPlus.VehicleGadgets
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Rage;
    
    using VehicleGadgetsPlus.VehicleGadgets.XML;

    internal sealed class Ladder : VehicleGadget
    {
        private readonly LadderEntry ladderDataEntry;
        private readonly VehicleBone ladderBase,
                                     ladderMain,
                                     ladderBucket;
        private readonly Extension[] ladderExtensions;
        
        public bool HasBase => ladderBase != null;
        public bool HasMain => ladderMain != null;
        public bool HasExtensions => ladderExtensions != null;
        public bool HasBucket => ladderBucket != null;

        private readonly string loopSoundId;
        private readonly string endSoundId;
        private bool shouldPlayLoopSound;

        public Ladder(Vehicle vehicle, VehicleGadgetEntry dataEntry) : base(vehicle, dataEntry)
        {
            ladderDataEntry = (LadderEntry)dataEntry;
            
            if (ladderDataEntry.HasBase)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Base.BoneName, out ladderBase))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Base.BoneName}\" for the Ladder Base");
                }
            }

            if (ladderDataEntry.HasMain)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Main.BoneName, out ladderMain))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Main.BoneName}\" for the Ladder Main");
                }
            }

            if (ladderDataEntry.HasExtensions)
            {
                ladderExtensions = new Extension[ladderDataEntry.Extensions.Parts.Length];

                for (int i = 0; i < ladderDataEntry.Extensions.Parts.Length; i++)
                {
                    if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Extensions.Parts[i].BoneName, out VehicleBone extensionBone))
                    {
                        throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Extensions.Parts[i].BoneName}\" for the Ladder Extension #{i}");
                    }

                    ladderExtensions[i] = new Extension(this, ladderDataEntry.Extensions.Parts[i], ladderDataEntry.Extensions.Direction, extensionBone);
                }
            }

            if (ladderDataEntry.HasBucket)
            {
                if (!VehicleBone.TryGetForVehicle(vehicle, ladderDataEntry.Bucket.BoneName, out ladderBucket))
                {
                    throw new InvalidOperationException($"The model \"{vehicle.Model.Name}\" doesn't have the bone \"{ladderDataEntry.Bucket.BoneName}\" for the Ladder Bucket");
                }
            }

            if (ladderDataEntry.HasSoundsSet)
            {
                loopSoundId = $"ladder_loop_{Guid.NewGuid()}";
                endSoundId = $"ladder_end_{Guid.NewGuid()}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (ladderDataEntry.HasSoundsSet)
            {
                Plugin.SoundPlayer.Clean(loopSoundId);
                Plugin.SoundPlayer.Clean(endSoundId);
            }
            base.Dispose(disposing);
        }

        public override void Update(bool isPlayerIn)
        {
            if (!isPlayerIn)
                return;

            shouldPlayLoopSound = false;

            // left/right
            if (HasBase)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad6))
                {
                    RotateBaseRight();
                }
                else if (Game.IsKeyDownRightNow(Keys.NumPad4))
                {
                    RotateBaseLeft();
                }
            }
            
            // up/down
            if (HasMain)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad8))
                {
                    RotateMainUp();
                }
                else if (Game.IsKeyDownRightNow(Keys.NumPad2))
                {
                    RotateMainDown();
                }
            }

            // extend
            if (HasExtensions)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad9))
                {
                    ExtendLadder();
                }
                else if (Game.IsKeyDownRightNow(Keys.NumPad3))
                {
                    RetractLadder();
                }
            }

            // bucket up/down
            if (HasBucket)
            {
                if (Game.IsKeyDownRightNow(Keys.NumPad7))
                {
                    RotateBucketUp();
                }
                else if (Game.IsKeyDownRightNow(Keys.NumPad1))
                {
                    RotateBucketDown();
                }
            }

            if (ladderDataEntry.HasSoundsSet)
            {
                if (shouldPlayLoopSound)
                {
                    if (!IsPlayingLoopSound())
                    {
                        if (IsPlayingEndSound())
                        {
                            StopEndSound();
                        }
                        
                        PlayLoopSound();
                    }
                }
                else if (IsPlayingLoopSound())
                {
                    StopLoopSound();
                    PlayEndSound();
                }
            }
        }

        private bool IsPlayingLoopSound()
        {
            return Plugin.SoundPlayer.IsPlaying(loopSoundId);
        }

        private void PlayLoopSound()
        {
            if (!ladderDataEntry.HasSoundsSet)
                return;

            if(ladderDataEntry.SoundsSet.IsDefaultLoop)
            {
                Plugin.SoundPlayer.Play(loopSoundId, true, ladderDataEntry.SoundsSet.NormalizedVolume, () => Properties.Resources.default_ladder_loop);
            }
            else
            {
                Plugin.SoundPlayer.Play(loopSoundId, true, ladderDataEntry.SoundsSet.NormalizedVolume, () => ladderDataEntry.SoundsSet.LoopSoundFilePath);
            }
        }

        private void StopLoopSound()
        {
            if (!ladderDataEntry.HasSoundsSet)
                return;

            Plugin.SoundPlayer.Stop(loopSoundId);
        }

        private bool IsPlayingEndSound()
        {
            return Plugin.SoundPlayer.IsPlaying(endSoundId);
        }

        private void PlayEndSound()
        {
            if (!ladderDataEntry.HasSoundsSet)
                return;

            if (ladderDataEntry.SoundsSet.IsDefaultEnd)
            {
                Plugin.SoundPlayer.Play(endSoundId, false, ladderDataEntry.SoundsSet.NormalizedVolume, () => Properties.Resources.default_ladder_end);
            }
            else
            {
                Plugin.SoundPlayer.Play(endSoundId, false, ladderDataEntry.SoundsSet.NormalizedVolume, () => ladderDataEntry.SoundsSet.EndSoundFilePath);
            }
        }

        private void StopEndSound()
        {
            if (!ladderDataEntry.HasSoundsSet)
                return;

            Plugin.SoundPlayer.Stop(endSoundId);
        }

        private void RotateBaseLeft()
        {
            if (!HasBase)
                return;

            Vector3 axis = ladderDataEntry.Base.RotationAxis;
            float degrees = ladderDataEntry.Base.RotationSpeed * Game.FrameTime;
            ladderBase.RotateAxis(axis, degrees);

            shouldPlayLoopSound = true;
        }

        private void RotateBaseRight()
        {
            if (!HasBase)
                return;

            Vector3 axis = ladderDataEntry.Base.RotationAxis;
            float degrees = -ladderDataEntry.Base.RotationSpeed * Game.FrameTime;
            ladderBase.RotateAxis(axis, degrees);

            shouldPlayLoopSound = true;
        }

        private void RotateMainUp()
        {
            if (!HasMain)
                return;

            Matrix m = ladderMain.Matrix;
            float angle = GetAngle(m, ladderMain.OriginalRotation, ladderDataEntry.Main.RotationAxis);

            if (angle < ladderDataEntry.Main.MaxAngle)
            {
                Vector3 axis = ladderDataEntry.Main.RotationAxis;
                float degrees = ladderDataEntry.Main.RotationSpeed * Game.FrameTime;
                ladderMain.RotateAxis(axis, degrees);

                shouldPlayLoopSound = true;
            }
        }

        private void RotateMainDown()
        {
            if (!HasMain)
                return;

            Matrix m = ladderMain.Matrix;
            float angle = GetAngle(m, ladderMain.OriginalRotation, ladderDataEntry.Main.RotationAxis);

            if (angle > ladderDataEntry.Main.MinAngle)
            {
                Vector3 axis = ladderDataEntry.Main.RotationAxis;
                float degrees = -ladderDataEntry.Main.RotationSpeed * Game.FrameTime;
                ladderMain.RotateAxis(axis, degrees);

                shouldPlayLoopSound = true;
            }
        }

        private void ExtendLadder()
        {
            if (!HasExtensions)
                return;

            for (int i = 0; i < ladderExtensions.Length; i++)
            {
                if (ladderExtensions[i].Extend())
                {
                    shouldPlayLoopSound = true;
                }
            }
        }

        private void RetractLadder()
        {
            if (!HasExtensions)
                return;

            for (int i = 0; i < ladderExtensions.Length; i++)
            {
                if (ladderExtensions[i].Retract())
                {
                    shouldPlayLoopSound = true;
                }
            }
        }

        private void RotateBucketUp()
        {
            if (!HasBucket)
                return;
            
            Matrix m = ladderBucket.Matrix;
            float angle = GetAngle(m, ladderBucket.OriginalRotation, ladderDataEntry.Bucket.RotationAxis);

            if (angle < ladderDataEntry.Bucket.MaxAngle)
            {
                Vector3 axis = ladderDataEntry.Bucket.RotationAxis;
                float degrees = ladderDataEntry.Bucket.RotationSpeed * Game.FrameTime;
                ladderBucket.RotateAxis(axis, degrees);

                shouldPlayLoopSound = true;
            }
        }

        private void RotateBucketDown()
        {
            if (!HasBucket)
                return;

            Matrix m = ladderBucket.Matrix;
            float angle = GetAngle(m, ladderBucket.OriginalRotation, ladderDataEntry.Bucket.RotationAxis);

            if (angle > ladderDataEntry.Bucket.MinAngle)
            {
                Vector3 axis = ladderDataEntry.Bucket.RotationAxis;
                float degrees = -ladderDataEntry.Bucket.RotationSpeed * Game.FrameTime;
                ladderBucket.RotateAxis(axis, degrees);

                shouldPlayLoopSound = true;
            }
        }

        private float GetAngle(Matrix matrix, Quaternion originalRotation, Vector3 axis)
        {
            matrix.Decompose(out _, out Quaternion rotation, out _);

            Vector3 rotationVector = rotation.ToVector();
            Vector3 origRotationVector = originalRotation.ToVector();
            Vector3 planeNormal = Vector3.Cross(axis, origRotationVector);

            float angle = MathHelper.ConvertRadiansToDegrees(Math.Asin(Vector3.Dot(planeNormal, rotationVector) / (planeNormal.Length() * rotationVector.Length())));

            return angle;
        }



        private class Extension
        {
            private readonly LadderEntry.LadderExtension extensionData;
            private readonly Ladder owner;
            private readonly VehicleBone bone;
            private readonly Vector3 direction;

            public float MaxDistanceSqr { get; }

            public float CurrentDistanceSqr
            {
                get
                {
                    Matrix matrix = bone.Matrix;
                    Vector3 translation = MatrixUtils.DecomposeTranslation(matrix);

                    return Vector3.DistanceSquared(translation, bone.OriginalTranslation);
                }
            }

            public Extension(Ladder owner, LadderEntry.LadderExtension data, Vector3 direction, VehicleBone bone)
            {
                this.owner = owner;
                extensionData = data;
                this.direction = direction;
                this.bone = bone;

                MaxDistanceSqr = extensionData.ExtensionDistance * extensionData.ExtensionDistance;
            }


            public bool Extend()
            {
                if (CurrentDistanceSqr >= MaxDistanceSqr)
                    return false;
                
                float moveDist = extensionData.MoveSpeed * Game.FrameTime;
                Vector3 translation = direction * moveDist;

                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(translation) * bone.Matrix;
                float newDistanceSqr = Vector3.DistanceSquared(MatrixUtils.DecomposeTranslation(newMatrix), bone.OriginalTranslation);

                // check if the new matrix overpasses the distance limits
                if (newDistanceSqr > MaxDistanceSqr)
                    return false;

                bone.Translate(translation);
                return true;
            }

            public bool Retract()
            {
                float currentDistanceSqr = CurrentDistanceSqr;
                if (currentDistanceSqr < 0.015f * 0.015f)
                    return false;
                
                float moveDist = extensionData.MoveSpeed * Game.FrameTime;
                Vector3 translation = -direction * moveDist;

                Matrix newMatrix = Matrix.Scaling(1.0f, 1.0f, 1.0f) * Matrix.Translation(translation) * bone.Matrix;
                float newDistanceSqr = Vector3.DistanceSquared(MatrixUtils.DecomposeTranslation(newMatrix), bone.OriginalTranslation);

                // check if the new matrix overpasses the distance limits
                if (newDistanceSqr > currentDistanceSqr)
                    return false;

                bone.Translate(translation);
                return true;
            }
        }
    }
}
