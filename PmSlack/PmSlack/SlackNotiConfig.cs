using System.Configuration;

namespace PmSlack
{
	public interface ISlackNoti
	{
		string ProcessPath { get; set; }
		string Url { get; set; }
		string Channel { get; set; }
		string UserName { get; set; }
	}

	public class SlackNotiElement : ConfigurationElement, ISlackNoti
	{
		[ConfigurationProperty("processPath", IsRequired = true)]
		public string ProcessPath
		{
			get { return (string)this["processPath"]; }
			set { this["processPath"] = value; }
		}

		[ConfigurationProperty("url", IsRequired = true)]
		public string Url
		{
			get { return (string)this["url"]; }
			set { this["url"] = value; }
		}

		[ConfigurationProperty("channel", IsRequired = true)]
		public string Channel
		{
			get { return (string)this["channel"]; }
			set { this["channel"] = value; }
		}

		[ConfigurationProperty("userName", IsRequired = true)]
		public string UserName
		{
			get { return (string)this["userName"]; }
			set { this["userName"] = value; }
		}

		[ConfigurationProperty("appName", IsRequired = true)]
		public string AppName
		{
			get { return (string)this["appName"]; }
			set { this["appName"] = value; }
		}
	}

	[ConfigurationCollection(typeof(SlackNotiElement), AddItemName = "noti", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class SlackNotiCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName
		{
			get
			{
				return "noti";
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new SlackNotiElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SlackNotiElement)element).ProcessPath;
		}

		public SlackNotiElement this[int index]
		{
			get
			{
				return BaseGet(index) as SlackNotiElement;
			}
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}
	}

	public class SlackNotiConfig : ConfigurationSection
	{
		private static SlackNotiConfig _slackNotiConfig = (SlackNotiConfig)ConfigurationManager.GetSection("slackNoti");
		public static SlackNotiConfig Config { get { return _slackNotiConfig; } }

		[ConfigurationProperty("notis")]
		public SlackNotiCollection Notis
		{
			get { return (SlackNotiCollection)base["notis"]; }
		}
	}
}
