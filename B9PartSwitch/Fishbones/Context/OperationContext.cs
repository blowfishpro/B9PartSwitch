namespace B9PartSwitch.Fishbones.Context
{
    public class OperationContext
    {
        public readonly Operation Operation;
        public readonly OperationContext ParentOperation;
        public readonly object Subject;

        public OperationContext(Operation operation, object subject)
        {
            subject.ThrowIfNullArgument(nameof(subject));

            Operation = operation;
            Subject = subject;
        }

        public OperationContext(OperationContext parentOperation, object subject)
        {
            parentOperation.ThrowIfNullArgument(nameof(parentOperation));
            subject.ThrowIfNullArgument(nameof(subject));

            ParentOperation = parentOperation;
            Operation = ParentOperation.Operation;
            Subject = subject;
        }

        public object Parent => ParentOperation?.Subject;

        public object Root => ParentOperation?.Root ?? Subject;
    }
}
