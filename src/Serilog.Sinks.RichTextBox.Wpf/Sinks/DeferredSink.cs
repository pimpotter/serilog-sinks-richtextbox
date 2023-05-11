using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.RichTextBox.Themes;
using System;
using System.Windows.Threading;
using System.Windows.Controls;
using Serilog.Sinks.RichTextBox.Abstraction;
using Serilog.Sinks.RichTextBox.Output;
using Serilog.Sinks.RichTextBox;

namespace Serilog
{
    public class DeferredSink : ILogEventSink
    {
        private ILogEventSink _attachedSink;
        private readonly object _syncRoot = new object();
        private readonly string outputTemplate;
        private readonly IFormatProvider formatProvider = null;
        private readonly RichTextBoxTheme theme = null;
        private readonly DispatcherPriority dispatcherPriority = DispatcherPriority.Background;

        public DeferredSink(string outputTemplate, IFormatProvider formatProvider, RichTextBoxTheme theme, DispatcherPriority dispatcherPriority)
        {
            this.outputTemplate = outputTemplate;
            this.formatProvider = formatProvider;
            this.theme = theme;
            this.dispatcherPriority = dispatcherPriority;
        }

        public void Attach(RichTextBox richTextBoxControl)
        {
            lock (_syncRoot)
            {
                if (_attachedSink == null)
                {
                    var appliedTheme = theme ?? RichTextBoxConsoleThemes.Literate;

                    var formatter = new XamlOutputTemplateRenderer(appliedTheme, outputTemplate, formatProvider);

                    var richTextBox = new RichTextBoxImpl(richTextBoxControl);

                    var richTextBoxSink = new RichTextBoxSink(richTextBox, formatter, dispatcherPriority, _syncRoot);
                    _attachedSink = richTextBoxSink;
                }
                else throw new System.Exception("Sink already attached");
            }
        }

        public void Detach()
        {
            lock (_syncRoot)
            {
                _attachedSink = null;
            }
        }

        public void Emit(LogEvent logEvent)
        {
            _attachedSink?.Emit(logEvent);
        }
    }

}
