using System.Text.Json;

namespace DupIQ.IssueIdentity
{
	public class PropStuffer
	{
		private Dictionary<string, object> propertyBag = new Dictionary<string, object>();

		public Dictionary<string, object> PropertyBag
		{
			get { return this.propertyBag; }
			set { this.propertyBag = value; }
		}

		public void AddProperties(object properties)
		{
			propertyBag.Add(properties.GetType().Name, properties);
		}

		public T GetThingFromProperties<T>()
		{
			string json = JsonSerializer.Serialize(propertyBag[typeof(T).Name]);
			return JsonSerializer.Deserialize<T>(json);
		}
	}
}
