using System;
using System.Text;

namespace B9PartSwitch.Utils
{
    public class GroupedStringBuilder
    {
        private enum State
        {
            Initial,
            TextWritten,
            WaitingForNewline,
            WaitingForGroup,
        }

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private State state = State.Initial;

        public void BeginGroup()
        {
            if (state != State.Initial) state = State.WaitingForGroup;
        }

        public void Append(string value)
        {
            CheckState();
            stringBuilder.Append(value);
            state = State.TextWritten;
        }

        public void Append(string value, object arg0)
        {
            CheckState();
            stringBuilder.AppendFormat(value, arg0);
            state = State.TextWritten;
        }

        public void Append(string format, object arg0, object arg1)
        {
            CheckState();
            stringBuilder.AppendFormat(format, arg0, arg1);
            state = State.TextWritten;
        }

        public void Append(string format, object arg0, object arg1, object arg2)
        {
            CheckState();
            stringBuilder.AppendFormat(format, arg0, arg1, arg2);
            state = State.TextWritten;
        }

        public void AppendLine()
        {
            CheckState();
            state = State.WaitingForNewline;
        }

        public void AppendLine(string value)
        {
            CheckState();
            stringBuilder.AppendFormat(value);
            state = State.WaitingForNewline;
        }

        public void AppendLine(string format, object arg0)
        {
            CheckState();
            stringBuilder.AppendFormat(format, arg0);
            state = State.WaitingForNewline;
        }

        public void AppendLine(string format, object arg0, object arg1)
        {
            CheckState();
            stringBuilder.AppendFormat(format, arg0, arg1);
            state = State.WaitingForNewline;
        }

        public override string ToString() => stringBuilder.ToString();

        public void Clear()
        {
            stringBuilder.Length = 0;
            state = State.Initial;
        }

        private void CheckState()
        {
            if (state == State.WaitingForGroup)
                stringBuilder.Append("\n\n");
            else if (state == State.WaitingForNewline)
                stringBuilder.Append("\n");
        }
    }
}
