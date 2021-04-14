﻿using System.Threading.Tasks;
using eazy.sms.Common;
using eazy.sms.Core.Exceptions;
using eazy.sms.Core.Helper;
using eazy.sms.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace eazy.sms.Core
{
    public class Notifiable<T>
    {
        /// <summary>
        ///     No Render Found Message
        /// </summary>
        private const string NoRenderFoundMessage
            = "Please use one of the available methods for specifying how to render your sms (e.g. Text() or Template())";

        /// <summary>
        ///     Required: This is the sender name.
        ///     The maximum length is 11 alphanumerical characters.
        /// </summary>
        [JsonProperty("from")]
        private string _From { get; set; }

        /// <summary>
        ///     Required: The destination mobile numbers.
        /// </summary>
        [JsonProperty("to")]
        private string[] _SmsRecipients { get; set; }

        /// <summary>
        ///     The allowed channels field forces a message to only use certain routes.
        /// </summary>
        [JsonProperty(
            DefaultValueHandling = DefaultValueHandling.Ignore,
            PropertyName = "allowedChannels",
            ItemConverterType = typeof(StringEnumConverter)
        )]
        private SMSChannel _SmsChannel { get; set; }

        /// <summary>
        ///     Template data to pass to the Txt view to render.
        /// </summary>
        private T _TemplateModel { get; set; }

        /// <summary>
        ///     The Text template to use to generate the message.
        /// </summary>
        private string _TemplatePath { get; set; }

        /// <summary>
        ///     Required: The actual text body of the message.
        /// </summary>
        [JsonProperty("body")]
        private Body _Body { get; set; }

        /// <summary>
        ///     Optional : FileName.ext && Path to file
        /// </summary>
        [JsonProperty("attachment")]
        private Attachment _Attachment { get; set; }

        /// <summary>
        ///     Optional : The title of the message
        /// </summary>
        /// [JsonProperty("subject")]
        private string _Subject { get; set; }

        /// <summary>
        /// </summary>
        private bool _IsSchedule { get; set; }

        /// <summary>
        /// </summary>
        private string _ScheduleDate { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        protected Notifiable<T> From(string from)
        {
            _From = from;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        public Notifiable<T> Recipient(string[] recipients)
        {
            _SmsRecipients = recipients;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        public Notifiable<T> Subject(string subject)
        {
            _Subject = subject;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public Notifiable<T> Attach(Attachment attachment)
        {
            _Attachment = attachment;
            return this;
        }

        public Notifiable<T> Schedule(bool isSchedule = false, string scheduleDate = null)
        {
            _IsSchedule = isSchedule;
            _ScheduleDate = scheduleDate;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Notifiable<T> Body(Body body)
        {
            _Body = body;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public Notifiable<T> Channel(SMSChannel channel)
        {
            _SmsChannel = channel;
            return this;
        }

        /// <summary>
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="templateModel"></param>
        /// <returns></returns>
        public Notifiable<T> Template(string templatePath, T templateModel = default)
        {
            _TemplateModel = templateModel;
            _TemplatePath = templatePath;
            return this;
        }

        protected virtual void Boot()
        {
        }

        internal async Task SendAsync(INotification notification)
        {
            Boot();

            //logic here
            var msg = await BuildMsg()
                .ConfigureAwait(false);

            await notification.NotifyAsync(
                msg,
                _Subject,
                _SmsRecipients,
                _From,
                _ScheduleDate,
                _IsSchedule,
                _Attachment
            ).ConfigureAwait(true);
        }

        /// <summary>
        ///     Prepare message
        /// </summary>
        /// <returns></returns>
        private async Task<string> BuildMsg()
        {
            if (_Body != null) return _Body.Content;

            if (_TemplatePath != null)
                return await TemplateRenderer.RenderTemplateToStringAsync(_TemplatePath, _TemplateModel)
                    .ConfigureAwait(false);

            throw new NoSmsRendererFound(NoRenderFoundMessage);
        }
    }
}