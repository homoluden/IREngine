using IRGame.Characters;
using IRGame.Common.Enums;

namespace IRGame.Common.Helpers
{
    public static class CharacterHelper
    {

        public static void ApplyForce(this Character container, double fx, double fy)
        {
            container.State[(int)CharacterState.XForce] = fx;
            container.State[(int)CharacterState.YForce] = fy;
        }

        public static void ApplyMoment(this Character container, double mx, double my)
        {
            container.State[(int)CharacterState.XMoment] = mx;
            container.State[(int)CharacterState.YMoment] = my;
        }

    }
}
