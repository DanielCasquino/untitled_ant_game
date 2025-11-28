using UnityEngine;

public class LegConstraint : ForeignConstraint
{
    [SerializeField] RotationConstraint[] mustBeGrounded;
    public override bool Evaluate()
    {
        foreach (RotationConstraint mbg in mustBeGrounded)
            if (mbg.interpolating)
                return false;
        return true;
    }
}