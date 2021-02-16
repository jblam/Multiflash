using System;

namespace JBlam.Multiflash.Serial
{
    enum MessageDirection
    {
        ToRemote = '>',
        FromRemote = '<',
    }
    struct Message
    {
        public MessageDirection Direction { get; init; }
        public string Content { get; init; }
        public bool IsTerminated { get; init; }
        public TimeSpan StartedAt { get; init; }

        public override string ToString() => $"{StartedAt.TotalSeconds:F3} {(char)Direction} [{Content}]{(IsTerminated ? " (\\n)" : "")}";

        public static Message CreateOutgoing(string content, TimeSpan time) => new Message
        {
            Direction = MessageDirection.ToRemote,
            Content = content,
            // JB 2021-02-15: always treat outgoing messages as terminated;
            // we assume the user does not intend them to be concatenated
            IsTerminated = true,
            StartedAt = time,
        };
        public static Message ConsumeIncoming(ref ReadOnlySpan<char> input, TimeSpan time)
        {
            var newline = input.IndexOf('\n');
            string content;
            bool isTerminated = newline >= 0;
            if (isTerminated)
            {
                content = input[..newline].TrimEnd('\r').ToString();
                input = input[(newline + 1)..];
            }
            else
            {
                content = input.ToString();
                input = input[input.Length..];
            }
            return new Message
            {
                Content = content,
                Direction = MessageDirection.FromRemote,
                IsTerminated = isTerminated,
                StartedAt = time,
            };
        }

        public Message? TryCombine(Message other)
        {
            if (Direction == other.Direction && !IsTerminated)
            {
                return new Message
                {
                    Direction = Direction,
                    Content = Content + other.Content,
                    IsTerminated = other.IsTerminated,
                    StartedAt = StartedAt,
                };
            }
            else
            {
                return null;
            }
        }
    }
}
