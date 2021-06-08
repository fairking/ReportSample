using Stimulsoft.Base;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Data.Engine;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace ReportGenTest
{
    public class CustomStiJsonDatabase : StiJsonDatabase
    {
        public CustomStiJsonDatabase() : base() { }

        public CustomStiJsonDatabase(string name, string pathData) : base(name, pathData) { }

        public CustomStiJsonDatabase(string name, string pathData, string key) : base(name, pathData, key) { }

        private IDictionary<string, string> _customHeaders;
        public IDictionary<string, string> CustomHeaders 
        { 
            get
            {
                return _customHeaders ?? (_customHeaders = new Dictionary<string, string>());
            }
        }

        public bool ThrowConnectionException { get; set; }

        public override void RegData(StiDictionary dictionary, bool loadData)
        {
            DataSet dataSet = null;

            try
            {
                StiDataLoaderHelper.Data data = LoadSingle(dictionary.Report, ParsePath(base.PathData, dictionary.Report));
                if (data != null && data.Array != null)
                {
                    dataSet = FillData(dictionary, (StiBaseOptions.DefaultJsonConverterVersion == StiJsonConverterVersion.ConverterV2) ? StiJsonToDataSetConverterV2.GetDataSet(data.Array, RelationDirection) : StiJsonToDataSetConverter.GetDataSet(data.Array), this);
                }
            }
            catch (Exception e)
            {
                if (!StiRenderingMessagesHelper.WriteConnectionException(dictionary, base.Name, e) || ThrowConnectionException)
                {
                    throw;
                }
            }

            RegDataSetInDataStore(dictionary, dataSet);
        }

        private DataSet FillData(StiDictionary dictionary, DataSet dataSet, StiJsonDatabase database)
        {
            return dataSet;
        }

        private StiDataLoaderHelper.Data LoadSingle(StiReport report, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (StiHyperlinkProcessor.IsResourceHyperlink(path))
            {
                return new StiDataLoaderHelper.Data(StiHyperlinkProcessor.GetResourceNameFromHyperlink(path), GetBytes(report, path, cookieContainer: report?.CookieContainer));
            }

            return LoadSingle(path, report?.CookieContainer);
        }

        private StiDataLoaderHelper.Data LoadSingle(string path, CookieContainer cookieContainer = null)
        {
            if (!string.IsNullOrEmpty(path) && !path.StartsWith("http://") && !path.StartsWith("https://") && File.Exists(path))
            {
                return new StiDataLoaderHelper.Data(Path.GetFileNameWithoutExtension(path), File.ReadAllBytes(path));
            }

            Uri uri = new Uri(path);

            return new StiDataLoaderHelper.Data(Path.GetFileNameWithoutExtension(uri.LocalPath), LoadFromUrl(path, cookieContainer));
        }

        private byte[] GetBytes(StiReport report, string hyperlink, bool firstPositionInDataSource = false, bool allowDataLoading = false, CookieContainer cookieContainer = null)
        {
            string resourceName = StiHyperlinkProcessor.GetResourceNameFromHyperlink(hyperlink);
            if (resourceName != null)
            {
                return GetResource(report, resourceName)?.Content;
            }

            string variableName = StiHyperlinkProcessor.GetVariableNameFromHyperlink(hyperlink);
            if (variableName != null)
            {
                StiVariable variable = GetVariable(report, variableName);
                if (variable != null)
                {
                    if (variable.ValueObject is Image)
                    {
                        return StiImageConverter.ImageToBytes(variable.ValueObject as Image, allowNulls: true);
                    }
                    if (variable.ValueObject is byte[] && StiImageHelper.IsImage(variable.ValueObject as byte[]))
                    {
                        return variable.ValueObject as byte[];
                    }
                }
                return null;
            }

            string dataColumnName = StiHyperlinkProcessor.GetDataColumnNameFromHyperlink(hyperlink);
            if (dataColumnName != null)
            {
                if (allowDataLoading)
                {
                    byte[] bytes = GetBytesFromColumnWithLoading(report, dataColumnName);
                    if (bytes != null)
                    {
                        return bytes;
                    }
                }
                return StiDataColumn.GetDatasFromDataColumn(report.Dictionary, dataColumnName, null, firstPositionInDataSource).FirstOrDefault() as byte[];
            }

            string file = StiHyperlinkProcessor.GetFileNameFromHyperlink(hyperlink);
            if (file != null)
            {
                if (!File.Exists(file))
                {
                    return null;
                }
                return File.ReadAllBytes(file);
            }

            return LoadFromUrl(hyperlink);
        }

        private StiVariable GetVariable(StiReport report, string variableName)
        {
            if (report == null || string.IsNullOrWhiteSpace(variableName))
            {
                return null;
            }
            variableName = variableName.ToLowerInvariant().Trim();
            return report.Dictionary.Variables.ToList().FirstOrDefault((StiVariable v) => v.Name != null && v.Name.ToLowerInvariant().Trim() == variableName);
        }

        private StiResource GetResource(StiReport report, string resourceName)
        {
            if (report == null || string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }
            resourceName = resourceName.ToLowerInvariant().Trim();
            return report.Dictionary.Resources.ToList().FirstOrDefault((StiResource r) => r.Name != null && r.Name.ToLowerInvariant().Trim() == resourceName);
        }

        private byte[] GetBytesFromColumnWithLoading(StiReport report, string dataColumnName)
        {
            StiDataSource dataSource = StiDataColumn.GetDataSourceFromDataColumn(report.Dictionary, dataColumnName);
            if (dataSource == null)
            {
                return null;
            }
            DataTable dataTable = StiDataPicker.Fetch(report, dataSource);
            if (dataTable == null)
            {
                return null;
            }
            if (!dataTable.Columns.Contains(dataColumnName))
            {
                return null;
            }
            object obj = dataTable.Rows[0][dataColumnName];
            if (obj is string)
            {
                try
                {
                    obj = Convert.FromBase64String(obj as string);
                }
                catch
                {
                }
            }
            return obj as byte[];
        }

        private byte[] LoadFromUrl(string url, CookieContainer cookieContainer = null)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = StiBaseOptions.WebClientEncoding;
                wc.Credentials = CredentialCache.DefaultCredentials;
                // wc.Container = cookieContainer; needs override

                wc.Headers.Set(HttpRequestHeader.UserAgent, "Stimulsoft");

                _customHeaders?.ToList().ForEach(x => wc.Headers.Add(x.Key, x.Value));

                return wc.DownloadData(url);
            }
        }
    }
}
