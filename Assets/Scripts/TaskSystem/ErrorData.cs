using System;

namespace haw.pd20.tasksystem
{
    public readonly struct ErrorData
    {
        public ErrorType Type { get; }
        public DateTime DateTime { get; }
        public TaskNode Task { get; }
        public string Message { get; }

        public ErrorData(string message = "") : this(ErrorType.Default, null, message) { }
        
        public ErrorData(TaskNode task) : this(ErrorType.Default, task) { }

        public ErrorData(ErrorType type, TaskNode task, string message = "") : this()
        {
            Type = type;
            Task = task;
            Message = message;
            DateTime = DateTime.Now;
        }
    }
}