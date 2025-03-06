/*
 * Basis Theory API
 *
 * ## Getting Started * Sign-in to [Basis Theory](https://basistheory.com) and go to [Applications](https://portal.basistheory.com/applications) * Create a Basis Theory Private Application * All permissions should be selected * Paste the API Key into the `BT-API-KEY` variable
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using BasisTheory.net.Client;
using BasisTheory.net.Model;

namespace BasisTheory.net.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITokenizeApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <returns>Object</returns>
        Object Tokenize(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="requestOptions"></param>
        /// <returns>ApiResponse of Object</returns>
        ApiResponse<Object> TokenizeWithHttpInfo(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions));
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITokenizeApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Object</returns>
        System.Threading.Tasks.Task<Object> TokenizeAsync(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> TokenizeWithHttpInfoAsync(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ITokenizeApi : ITokenizeApiSync, ITokenizeApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class TokenizeApi : ITokenizeApi
    {
        private BasisTheory.net.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizeApi"/> class.
        /// </summary>
        /// <returns></returns>
        public TokenizeApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizeApi"/> class.
        /// </summary>
        /// <returns></returns>
        public TokenizeApi(string basePath)
        {
            this.Configuration = BasisTheory.net.Client.Configuration.MergeConfigurations(
                BasisTheory.net.Client.GlobalConfiguration.Instance,
                new BasisTheory.net.Client.Configuration { BasePath = basePath }
            );
            this.Client = new BasisTheory.net.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new BasisTheory.net.Client.ApiClient(this.Configuration.BasePath);
            this.ExceptionFactory = BasisTheory.net.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizeApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public TokenizeApi(BasisTheory.net.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = BasisTheory.net.Client.Configuration.MergeConfigurations(
                BasisTheory.net.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.Client = new BasisTheory.net.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new BasisTheory.net.Client.ApiClient(this.Configuration.BasePath);
            ExceptionFactory = BasisTheory.net.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenizeApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public TokenizeApi(BasisTheory.net.Client.ISynchronousClient client, BasisTheory.net.Client.IAsynchronousClient asyncClient, BasisTheory.net.Client.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = BasisTheory.net.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public BasisTheory.net.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public BasisTheory.net.Client.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public BasisTheory.net.Client.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public BasisTheory.net.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <returns>Object</returns>
        public Object Tokenize(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions))
        {
            BasisTheory.net.Client.ApiResponse<Object> localVarResponse = TokenizeWithHttpInfo(body, operationIndex, requestOptions);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <returns>ApiResponse of Object</returns>
        public BasisTheory.net.Client.ApiResponse<Object> TokenizeWithHttpInfo(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions))
        {
            if(requestOptions == default(BasisTheory.net.Client.RequestOptions))
            {
                requestOptions = new BasisTheory.net.Client.RequestOptions();
            }

            

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = BasisTheory.net.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                requestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = BasisTheory.net.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                requestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            requestOptions.Data = body;

            requestOptions.Operation = "TokenizeApi.Tokenize";
            requestOptions.OperationIndex = operationIndex;

            // authentication (ApiKey) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("BT-API-KEY")))
            {
                requestOptions.HeaderParameters.Add("BT-API-KEY", this.Configuration.GetApiKeyWithPrefix("BT-API-KEY"));
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/tokenize", requestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Tokenize", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Object</returns>
        public async System.Threading.Tasks.Task<Object> TokenizeAsync(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            BasisTheory.net.Client.ApiResponse<Object> localVarResponse = await TokenizeWithHttpInfoAsync(body, operationIndex, requestOptions, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <exception cref="BasisTheory.net.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="body"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="requestOptions"></param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Object)</returns>
        public async System.Threading.Tasks.Task<BasisTheory.net.Client.ApiResponse<Object>> TokenizeWithHttpInfoAsync(Object body = default(Object), int operationIndex = 0, BasisTheory.net.Client.RequestOptions requestOptions = default(BasisTheory.net.Client.RequestOptions), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {

            if(requestOptions == default(BasisTheory.net.Client.RequestOptions))
            {
                requestOptions = new BasisTheory.net.Client.RequestOptions();
            }

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = BasisTheory.net.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                requestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = BasisTheory.net.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                requestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            requestOptions.Data = body;

            requestOptions.Operation = "TokenizeApi.Tokenize";
            requestOptions.OperationIndex = operationIndex;

            // authentication (ApiKey) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("BT-API-KEY")))
            {
                requestOptions.HeaderParameters.Add("BT-API-KEY", this.Configuration.GetApiKeyWithPrefix("BT-API-KEY"));
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/tokenize", requestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("Tokenize", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

    }
}
