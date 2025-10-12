using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Framework.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroSystem.Dnn.BusinessEngine.Framework.Models;
using DotNetNuke.Entities.Portals;
using System.Text.RegularExpressions;

namespace NitroSystem.Dnn.BusinessEngine.Framework.Contracts
{
    public abstract class ServiceBase<T> : IService
    {
        protected ServiceDto Service { get; set; }
        protected IServiceWorker ServiceWorker { get; set; }
        protected virtual T Model { get; set; }
        protected virtual PortalSettings PortalSettings { get; set; }

        public event EventHandler<EventArgs> OnServiceSuccessEvent;
        public event EventHandler<EventArgs> OnServiceErrorEvent;
        public event EventHandler<EventArgs> OnServiceCompletedEvent;

        public void Init(IServiceWorker serviceWorker, ServiceDto service)
        {
            this.ServiceWorker = serviceWorker;
            this.Service = service;
            this.PortalSettings = new PortalSettings(PortalController.Instance.GetCurrentSettings().PortalId);

            try
            {
                if (!string.IsNullOrEmpty(service.Settings)) this.Model = JsonConvert.DeserializeObject<T>(service.Settings);
            }
            catch (Exception ex)
            {
                this.TryParseModel(service.Settings);
            }
        }

        public abstract bool TryParseModel(string serviceSettings);

        public abstract Task<ServiceResult> ExecuteAsync<T>();

        public void OnServiceSuccess()
        {
            OnServiceSuccessEvent?.Invoke(this, new EventArgs());
        }

        public void OnServiceError()
        {
            OnServiceErrorEvent?.Invoke(this, new EventArgs());
        }

        public void OnServiceCompleted()
        {
            OnServiceCompletedEvent?.Invoke(this, new EventArgs());
        }

        public string ParseParam(string text)
        {
            var matches = Regex.Matches(text, @"\@\w+", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var regexValue=match.Value.Trim();
                if ((regexValue ?? "").StartsWith("@") && this.Service.Params != null)
                {
                    var param = this.Service.Params.FirstOrDefault(p => p.ParamName == regexValue);
                    if (param != null && param.ParamValue != null) text = text.Replace(regexValue, param.ParamValue.ToString());
                }
            }

            return text;
        }
    }
}
