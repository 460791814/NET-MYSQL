using System;
namespace Model
{
	/// <summary>
	/// T_Soft:实体类(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
	public partial class E_Soft
	{
		public E_Soft()
		{}
		#region Model
		private int _id;
		private string _hash;
		private string _name;
		private long _length;
		private int? _hit;
		private int? _monthhit;
		private int? _weekhit;
		private int _softtype;
		private string _details;
		private int? _area;
		private string _publisher;
		private DateTime _updatetime;
		/// <summary>
		/// 主键
		/// </summary>
		public int ID
		{
			set{ _id=value;}
			get{return _id;}
		}
		/// <summary>
		/// hash码
		/// </summary>
		public string Hash
		{
			set{ _hash=value;}
			get{return _hash;}
		}
		/// <summary>
		/// 种子名称
		/// </summary>
		public string Name
		{
			set{ _name=value;}
			get{return _name;}
		}
		/// <summary>
		/// 种子大小 单位K
		/// </summary>
		public long Length
		{
			set{ _length=value;}
			get{return _length;}
		}
		/// <summary>
		/// 总点击
		/// </summary>
		public int? Hit
		{
			set{ _hit=value;}
			get{return _hit;}
		}
		/// <summary>
		/// 周点击
		/// </summary>
		public int? MonthHit
		{
			set{ _monthhit=value;}
			get{return _monthhit;}
		}
		/// <summary>
		/// 月点击
		/// </summary>
		public int? WeekHit
		{
			set{ _weekhit=value;}
			get{return _weekhit;}
		}
		/// <summary>
		/// 0未知类型，1，电影，2，图片，3，文档，4，程序
		/// </summary>
		public int SoftType
		{
			set{ _softtype=value;}
			get{return _softtype;}
		}
		/// <summary>
		/// 关键字
		/// </summary>
		public string Details
		{
			set{ _details=value;}
			get{return _details;}
		}
		/// <summary>
		/// 地区
		/// </summary>
		public int? Area
		{
			set{ _area=value;}
			get{return _area;}
		}
		/// <summary>
		/// 出版
		/// </summary>
		public string Publisher
		{
			set{ _publisher=value;}
			get{return _publisher;}
		}
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdateTime
		{
			set{ _updatetime=value;}
			get{return _updatetime;}
		}
		#endregion Model

	}
}

