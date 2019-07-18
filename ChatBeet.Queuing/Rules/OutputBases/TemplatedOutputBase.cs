﻿namespace ChatBeet.Queuing.Rules.OutputBases
{
    public class TemplatedOutputBase : IOutputBase, IViewable
    {
        public string Template { get; set; }

        public string GetOutput(IQueuedMessageSource message)
        {
            var output = Template;
            if (string.IsNullOrEmpty(output))
                return string.Empty;
            return ReplaceTemplateProperties(output, message);
        }

        private string ReplaceTemplateProperties(string input, IQueuedMessageSource message)
        {
            foreach (var prop in message.GetType().GetProperties())
                input = input.Replace($"{{{{{prop.Name}}}}}", prop.GetValue(message)?.ToString() ?? string.Empty);
            return input;
        }

        public ViewableNode ToNode() => new ViewableNode
        {
            Text = $"Interpolate \"{Template}\""
        };
    }
}
