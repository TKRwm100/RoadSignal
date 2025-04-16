using BveTypes.ClassWrappers;
using System;
using System.IO;

namespace BveEx.Toukaitetudou.RoadSignal
{
    public static class ConfigExtention
    {
        public static (BveTypes.ClassWrappers.Structure, BveTypes.ClassWrappers.Structure) GetStructure(this Structure structure, double location, string filename)
        {
            if (!File.Exists(Path.Combine(Path.GetDirectoryName(filename), structure.OnLightStructure)))
            {
                Diagnostics.ErrorDialog.Show(new Diagnostics.ErrorDialogInfo(nameof(RoadSignal), filename, $"ファイル {structure.OnLightStructure} が見つかりませんでした."));
            }
            bool isOffExists = !(structure.OffLightStructure is null) &&!File.Exists(Path.Combine(Path.GetDirectoryName(filename), structure.OffLightStructure));
            if (isOffExists)
            {
                Diagnostics.ErrorDialog.Show(new Diagnostics.ErrorDialogInfo(nameof(RoadSignal), filename, $"ファイル {structure.OffLightStructure} が見つかりませんでした."));

            }

            BveTypes.ClassWrappers.Structure onStr = new BveTypes.ClassWrappers.Structure(location, structure.Track, structure.X, structure.Y, structure.Z, structure.RX, structure.RY, structure.RZ, (TiltOptions)int.Parse(structure.Tilt), structure.Span, Model.FromXFile(Path.Combine(Path.GetDirectoryName(filename), structure.OnLightStructure)));
            return (onStr, structure.OffLightStructure is null ||isOffExists ? onStr : new BveTypes.ClassWrappers.Structure(location, structure.Track, structure.X, structure.Y, structure.Z, structure.RX, structure.RY, structure.RZ, (TiltOptions)int.Parse(structure.Tilt), structure.Span, Model.FromXFile(Path.Combine(Path.GetDirectoryName(filename), structure.OffLightStructure))));
        }

        public static TimeSpan GetTimeSpan(this DateTime dateTime)
        {
            return new TimeSpan(0, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }

    }
}