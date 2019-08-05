using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Rhino.Geometry;

namespace fSpy_Rhino.Core
{
    public static class BinaryParser
    {
        private static int MinimumByteLength = 16;

        /// <summary>
        /// Parses binary data of .fspy formar
        /// <see https://github.com/stuffmatic/fSpy/blob/develop/project_file_format.md />
        /// Note: 8 Bit = 1 Byte
        /// </summary>
        /// <param name="filename"></param>
        public static void Parse(string filename)
        {
            // create filestream
            FileStream fs = new FileStream(filename, FileMode.Open);

            // test valid file
            if (!IsValidFile(fs)) return;

            // read json size
            byte[] uint32Bytes = new byte[4];
            fs.Read(uint32Bytes, 0, 4);
            UInt32 jsonByteSize = BitConverter.ToUInt32(uint32Bytes, 0);

            // read image size
            fs.Read(uint32Bytes, 0, 4);
            UInt32 imageByteSize = BitConverter.ToUInt32(uint32Bytes, 0);

            // read json project state data
            byte[] jsonBytes = new byte[jsonByteSize];
            fs.Read(jsonBytes, 0, Convert.ToInt32(jsonByteSize));
            var jsonData = Encoding.UTF8.GetString(jsonBytes);
            JObject projectData = JObject.Parse(jsonData);

            // read image
            byte[] imageBytes = new byte[imageByteSize];
            fs.Read(imageBytes, 0, Convert.ToInt32(imageByteSize));
            var image = Image.FromStream(new MemoryStream(imageBytes));

            // parse
            ParseProjectDataJson(image, projectData);

        }

        private static bool IsValidFile(FileStream fs)
        {
            // minimum bytes required to write an empty .fspy file
            if (fs.Length < MinimumByteLength) return false;

            // parse first 4 bytes, they should spell "fspy" in hex
            byte[] idBytes = new byte[1];

            fs.Read(idBytes, 0, 1);
            var f = BitConverter.ToString(idBytes);
            fs.Read(idBytes, 0, 1);
            var s = BitConverter.ToString(idBytes);
            fs.Read(idBytes, 0, 1);
            var p = BitConverter.ToString(idBytes);
            fs.Read(idBytes, 0, 1);
            var y = BitConverter.ToString(idBytes);

            if (f != "66" || s != "73" || p != "70" || y != "79") return false;

            // parse next 4 bytes for version number, should be 1
            byte[] uint32Bytes = new byte[4];
            fs.Read(uint32Bytes, 0, 4);
            var projectFileVersion = BitConverter.ToUInt32(uint32Bytes, 0);
            return projectFileVersion == 1;
        }

        private static void ParseProjectDataJson(Image image, JObject jObject)
        {
            var width = image.Width;
            var height = image.Height;


            // calibrationSettingsBase
            // calibrationSettings1VP
            // calibrationSettings2VO
            // controlPointsStateBase

            var cpStateBaseObject = jObject.Property("controlPointsStateBase");
            var cpChildren = cpStateBaseObject.Children().Children().ToArray();
            var principalPoint = cpChildren[0];
            var origin = cpChildren[1];
            var referenceDistanceAnchor = cpChildren[2];
            var referenceDistanceHandleOffsets = cpChildren[3];
            var firstVanishingPoint = cpChildren[4];
            foreach (var child in firstVanishingPoint.Children().Children())
            {
                // TODO:
                // child is array of "x""y" children... how to enumerate??
                var lineSegments = new JArray(child.Children());
                var start = lineSegments[0];
                //var xStart = start.First;
                //var yStart = start.Last;

                var end = lineSegments[1];
                //var xEnd = end.First;
                //var yEnd = end.Last;

                //Line line = new Line(xStart.ToObject<double>(), yStart.ToObject<double>(), 0, xEnd.ToObject<double>(), yEnd.ToObject<double>(), 0);
            }

            // controlPointsState1VP
            // controlPointsState2VP
            // cameraParameters
        }
    }
}
