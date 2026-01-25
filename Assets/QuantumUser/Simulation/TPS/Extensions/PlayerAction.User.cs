namespace Quantum
{
    public partial struct PlayerActionComponent
    {
        //是否有action当前是活跃的,若大于0则为true
        public bool HasActiveAction => activeActionInfo.activeActionIndex >= 0;

        public ref PlayerActionInfo GetAction(ActionType actionType)
        {
            return ref actionInfoArray[(int)actionType];
        }

        //尝试获得当前活跃的action
        public bool TryGetActiveActionInfo(out PlayerActionInfo actionInfo)
        {
            //如果没有活跃的动作
            if (!HasActiveAction)
            {
                actionInfo = default;
                return false;
            }

            //如果有活跃的动作
            actionInfo = actionInfoArray[activeActionInfo.activeActionIndex];
            return true;
        }
    }
}
