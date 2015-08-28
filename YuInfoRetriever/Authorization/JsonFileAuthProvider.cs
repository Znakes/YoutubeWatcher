﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuInfoRetriever.Authorization
{
    public class JsonFileAuthProvider : IAuthProvider
    {
        public string JsonFileWithSecret { get; private set; }

        /// <summary>
        /// Ctor, expecting path to file with Json secret file 
        /// <remarks>
        /// You can get token from https://console.developers.google.com/project
        /// </remarks>
        /// </summary>
        /// <param name="filename"></param>
        public JsonFileAuthProvider()
        {}
        
        public Stream GetAuthDataStream()
        { 
            Contract.Requires(!String.IsNullOrEmpty(JsonFileWithSecret));

            return new FileStream(JsonFileWithSecret, FileMode.Open, FileAccess.Read);
        }

        public void SetParams(object param)
        {
            Contract.Requires(param!=null);
            Contract.Requires(File.Exists(param.ToString()));

            JsonFileWithSecret = param.ToString(); 
        }
    }
}