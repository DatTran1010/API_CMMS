using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace APITEST.Core.Reponse
{
	public class BaseResponse<TResponse>
	{
		public HttpStatusCode StatusCode { get; set; }
		public string Message { get; set; }
		public TResponse ResponseData { get; set; }
		public object ResponseHeader { get; set; }
		public bool IsSuccessStatusCode => StatusCode == HttpStatusCode.OK;
		public string ResourceKey { get; set; }
		public double TimeExecute { get; set; }
		public string TrackingMessage { get; set; }

		public static BaseResponse<TResponse> Success(TResponse data = default(TResponse), string message = "Success", string resourceKey = "SUCCESS", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.OK,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = message,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}
		public static BaseResponse<TResponse> InternalServerError(Exception ex, TResponse data = default(TResponse), string resourceKey = "INTERNAL_SERVER_ERROR", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.InternalServerError,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = ex.Message,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}

		public static BaseResponse<TResponse> InternalServerError(TResponse data = default(TResponse), string message = "Internal Server Error", string resourceKey = "INTERNAL_SERVER_ERROR", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.InternalServerError,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = message,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}

		public static BaseResponse<TResponse> BadRequest(TResponse data = default(TResponse), string message = "Bad Request", string resourceKey = "BAD_REQUEST", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.BadRequest,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = message,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}

		public static BaseResponse<TResponse> NotFound(TResponse data = default(TResponse), string message = "Data not found", string resourceKey = "DATA_NOT_FOUND", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.NotFound,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = message,
				ResponseHeader = null,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}

		public static BaseResponse<TResponse> NoContent(TResponse data = default(TResponse), string message = "No Content", string resourceKey = "NO_CONTENT", double timeExecute = 0, string trackingMessage = "")
		{
			return new BaseResponse<TResponse>()
			{
				StatusCode = HttpStatusCode.NoContent,
				ResponseData = data,
				ResourceKey = resourceKey,
				Message = message,
				TimeExecute = timeExecute,
				TrackingMessage = trackingMessage
			};
		}
	}
}
