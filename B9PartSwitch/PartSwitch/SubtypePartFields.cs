using UnityEngine;
using System.Collections.ObjectModel;

namespace B9PartSwitch
{
    public static class SubtypePartFields
    {
        public static readonly SubtypePartField<float> MaxTemp = new SubtypePartField<float>(
            "maxTemp",
            context => context.Subtype.maxTemp,
            maxTemp => maxTemp > 0f,
            part => (float)part.maxTemp,
            (context, maxTemp) => context.Part.maxTemp = maxTemp);

        public static readonly SubtypePartField<float> SkinMaxTemp = new SubtypePartField<float>(
            "skinMaxTemp",
            context => context.Subtype.skinMaxTemp,
            skinMaxTemp => skinMaxTemp > 0f,
            part => (float)part.skinMaxTemp,
            (context, skinMaxTemp) => context.Part.skinMaxTemp = skinMaxTemp);

        public static readonly SubtypePartField<float> CrashTolerance = new SubtypePartField<float>(
            "crashTolerance",
            context => context.Subtype.crashTolerance,
            crashTolerance => crashTolerance > 0f,
            part => part.crashTolerance,
            (context, crashTolerance) => context.Part.crashTolerance = crashTolerance);

        public static readonly SubtypePartField<AttachNode> SrfAttachNode = new SubtypePartField<AttachNode>(
            "attachNode",
            context => context.Subtype.attachNode,
            attachNode => attachNode.IsNotNull(),
            part => part.srfAttachNode,
            (context, attachNode) => {
                if (!context.Part.attachRules.allowSrfAttach || context.Part.srfAttachNode.IsNull() || attachNode.IsNull()) return;

                context.Part.srfAttachNode.position = attachNode.position * context.Module.Scale;
                context.Part.srfAttachNode.orientation = attachNode.orientation;
            });

        public static readonly SubtypePartField<Vector3> CoMOffset = new SubtypePartField<Vector3>(
            "CoMOffset",
            context => context.Subtype.CoMOffset,
            vector => vector.IsFinite(),
            part => part.CoMOffset,
            (context, vector) => context.Part.CoMOffset = vector);

        public static readonly SubtypePartField<Vector3> CoPOffset = new SubtypePartField<Vector3>(
            "CoPOffset",
            context => context.Subtype.CoPOffset,
            vector => vector.IsFinite(),
            part => part.CoPOffset,
            (context, vector) => context.Part.CoPOffset = vector);

        public static readonly SubtypePartField<Vector3> CoLOffset = new SubtypePartField<Vector3>(
            "CoLOffset",
            context => context.Subtype.CoLOffset,
            vector => vector.IsFinite(),
            part => part.CoLOffset,
            (context, vector) => context.Part.CoLOffset = vector);

        public static readonly SubtypePartField<Vector3> CenterOfBuoyancy = new SubtypePartField<Vector3>(
            "CenterOfBuoyancy",
            context => context.Subtype.CenterOfBuoyancy,
            vector => vector.IsFinite(),
            part => part.CenterOfBuoyancy,
            (context, vector) => context.Part.CenterOfBuoyancy = vector);

        public static readonly SubtypePartField<Vector3> CenterOfDisplacement = new SubtypePartField<Vector3>(
            "CenterOfDisplacement",
            context => context.Subtype.CenterOfDisplacement,
            vector => vector.IsFinite(),
            part => part.CenterOfDisplacement,
            (context, vector) => context.Part.CenterOfDisplacement = vector);

        public static readonly ReadOnlyCollection<ISubtypePartField> All = new ReadOnlyCollection<ISubtypePartField>(new ISubtypePartField[] {
            MaxTemp,
            SkinMaxTemp,
            CrashTolerance,
            SrfAttachNode,
            CoMOffset,
            CoPOffset,
            CoLOffset,
            CenterOfBuoyancy,
            CenterOfDisplacement
        });
    }
}
