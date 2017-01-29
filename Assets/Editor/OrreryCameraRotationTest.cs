using UnityEngine;
using UnityEditor;
using System;
using NUnit.Framework;

namespace SpaceBattles
{
    public class OrreryCameraRotationTest
    {
        private static readonly float AcceptableFloatError = 0.001f;

        public String
        DifferingResultsErrorMessage
            (String actual, String expected, String units)
        {
            return "Got " + actual + " " + units
                 + ", expecting " + expected + " " + units;
        }
        
        public void
        NoYInputTest
            (float xinput, float radius = 1000)
        {
            var ExpectedPosition = new Vector3(0, 0, radius);
            Vector2 InputAngles = new Vector2(xinput, 0);
            Vector3 CalculatedPosition
                = OrreryUIManager
                 .CalculateNewCameraPosition(InputAngles, radius);
            string ErrorMessage
                = DifferingResultsErrorMessage(
                    CalculatedPosition.ToString(),
                    ExpectedPosition.ToString(),
                    ""
                  );

            Assert.AreEqual(
                CalculatedPosition.x,
                ExpectedPosition.x,
                ErrorMessage
            );
            Assert.AreEqual(
                CalculatedPosition.y,
                ExpectedPosition.y,
                ErrorMessage
            );
            Assert.AreEqual(
                CalculatedPosition.z,
                ExpectedPosition.z,
                ErrorMessage
            );
        }

        public void
        NoXInputTest
            (float yinput, float radius = 1000)
        {
            Vector2 InputAngles = new Vector2(0, yinput);
            float YInputRadians = OrreryUIManager.ConvertToRadians(yinput);
            float ExpectedZ = Convert.ToSingle(radius * Math.Cos(YInputRadians));
            float ExpectedY = Convert.ToSingle(radius * Math.Sin(YInputRadians));
            var ExpectedPosition = new Vector3(0, ExpectedY, ExpectedZ);

            Vector3 CalculatedPosition
                = OrreryUIManager
                 .CalculateNewCameraPosition(InputAngles, radius);
            string ErrorMessage
                = DifferingResultsErrorMessage(
                    CalculatedPosition.ToString(),
                    ExpectedPosition.ToString(),
                    ""
                  );

            Assert.LessOrEqual(
                Math.Abs(CalculatedPosition.x - ExpectedPosition.x),
                AcceptableFloatError,
                ErrorMessage
            );
            Assert.LessOrEqual(
               Math.Abs(CalculatedPosition.y - ExpectedPosition.y),
               AcceptableFloatError,
               ErrorMessage
            );
            Assert.LessOrEqual(
               Math.Abs(CalculatedPosition.z - ExpectedPosition.z),
               AcceptableFloatError,
               ErrorMessage
            );
        }
        // --------------------------------------

        [Test] public void NoYInputX0Test()   { NoYInputTest(0);   }
        [Test] public void NoYInputX45Test()  { NoYInputTest(45);  }
        [Test] public void NoYInputX90Test()  { NoYInputTest(90);  }
        [Test] public void NoYInputX135Test() { NoYInputTest(135); }
        [Test] public void NoYInputX180Test() { NoYInputTest(180); }
        [Test] public void NoYInputX225Test() { NoYInputTest(225); }
        [Test] public void NoYInputX270Test() { NoYInputTest(270); }
        [Test] public void NoYInputX315Test() { NoYInputTest(315); }

        // --------------------------------------

        [Test] public void NoXInputY45Test()  { NoXInputTest(45);  }
        [Test] public void NoXInputY90Test()  { NoXInputTest(90);  }
        [Test] public void NoXInputY135Test() { NoXInputTest(135); }
        [Test] public void NoXInputY180Test() { NoXInputTest(180); }
        [Test] public void NoXInputY225Test() { NoXInputTest(225); }
        [Test] public void NoXInputY270Test() { NoXInputTest(270); }
        [Test] public void NoXInputY315Test() { NoXInputTest(315); }

        // --------------------------------------
    }
}