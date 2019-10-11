﻿
using System;
using System.Text;
using System.Linq.Expressions;

namespace TZM.XFramework.Data
{
    /// <summary>
    /// SQL 语句建造器
    /// </summary>
    public abstract class TextBuilder : ITextBuilder
    {
        protected string _escCharLeft;
        protected string _escCharRight;
        protected string _escCharQuote;
        protected StringBuilder _innerBuilder = null;
        protected IDbQueryProvider _provider = null;
        private ResolveToken _token = null;

        /// <summary>
        /// TAB 制表符
        /// </summary>
        public const string TAB = "    ";

        /// <summary>
        /// 获取或设置当前 <see cref="ITextBuilder"/> 对象的长度。
        /// </summary>
        public int Length
        {
            get { return _innerBuilder.Length; }
            set { _innerBuilder.Length = value; }
        }

        /// <summary>
        /// 获取或设置此实例中指定字符位置处的字符。
        /// </summary>
        public char this[int index] { get { return _innerBuilder[index]; } }

        /// <summary>
        /// 缩进
        /// </summary>
        public int Indent { get; set; }

        /// <summary>
        /// 参数化
        /// </summary>
        public virtual bool Parameterized
        {
            get { return _token != null && _token.Parameters != null; }
        }

        /// <summary>
        /// 解析上下文参数
        /// </summary>
        public ResolveToken Token
        {
            get { return _token; }
        }

        /// <summary>
        /// 实例化 <see cref="TextBuilder"/> 类的新实例
        /// </summary>
        /// <param name="provider">查询语义提供者</param>
        /// <param name="token">解析上下文参数</param>
        public TextBuilder(IDbQueryProvider provider, ResolveToken token)
        {
            _provider = provider;
            _token = token;
            _innerBuilder = new StringBuilder(128);
            _escCharLeft = _provider.QuotePrefix;
            _escCharRight = _provider.QuoteSuffix;
            _escCharQuote = _provider.SingleQuoteChar;
        }

        /// <summary>
        /// 追加列名
        /// </summary>
        /// <param name="aliases">表别名</param>
        /// <param name="expression">列名表达式</param>
        /// <returns>返回解析到的表别名</returns>
        public string AppendMember(TableAliasCache aliases, Expression expression)
        {
            Expression exp = expression;
            LambdaExpression lambdaExpression = exp as LambdaExpression;
            if (lambdaExpression != null) exp = lambdaExpression.Body;

            if (expression.CanEvaluate())
            {
                ConstantExpression c = expression.Evaluate();
                string value = _provider.Generator.GetSqlValue(c.Value, _token);
                _innerBuilder.Append(value);
                return value;
            }
            else
            {
                MemberExpression m = expression as MemberExpression;
                string alias = aliases == null ? null : aliases.GetTableAlias(m);
                this.AppendMember(alias, m.Member.Name);
                return alias;
            }
        }

        /// <summary>
        /// 追加列名
        /// </summary>
        /// <param name="expression">列名表达式</param>
        /// <param name="aliases">表别名</param>
        /// <returns>返回解析到的表别名</returns>
        public Expression AppendMember(Expression expression, TableAliasCache aliases)
        {
            this.AppendMember(aliases, expression);
            return expression;
        }

        /// <summary>
        /// 追加列名
        /// </summary>
        public ITextBuilder AppendMember(string alias, string name)
        {
            if (!string.IsNullOrEmpty(alias))
            {
                _innerBuilder.Append(alias);
                _innerBuilder.Append('.');
            }
            return this.AppendMember(name);
        }

        /// <summary>
        /// 追加成员名称
        /// </summary>
        /// <param name="name">成员名称</param>
        /// <param name="quote">使用安全符号括起来，临时表不需要括</param>
        /// <returns></returns>
        public virtual ITextBuilder AppendMember(string name, bool quote = true)
        {
            if (quote) _innerBuilder.Append(_escCharLeft);
            _innerBuilder.Append(name);
            if (quote) _innerBuilder.Append(_escCharRight);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加 AS
        /// </summary>
        public virtual ITextBuilder AppendAs(string name)
        {
            _innerBuilder.Append(" AS ");
            return this.AppendMember(name);
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(string value)
        {
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// 将字符串插入到此实例中的指定字符位置。
        /// </summary>
        public ITextBuilder Insert(int index, string value)
        {
            _innerBuilder.Insert(index, value);
            return this;
        }

        /// <summary>
        /// 将字符串插入到此实例中的指定字符位置。
        /// </summary>
        public ITextBuilder Insert(int index, object value)
        {
            _innerBuilder.Insert(index, value);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(int value)
        {
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(char value)
        {
            _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(object value)
        {
            if (value != null) _innerBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(object value, MemberExpression m)
        {
            var sql = _provider.Generator.GetSqlValue(value, _token, m);
            return this.Append(sql);
        }

        /// <summary>
        /// 在此实例的结尾追加指定字符串的副本。
        /// </summary>
        public ITextBuilder Append(object value, System.Reflection.MemberInfo m, Type declareType)
        {
            var sql = _provider.Generator.GetSqlValue(value, _token, m, declareType);
            return this.Append(sql);
        }

        /// <summary>
        /// 在此实例的结尾追加回车符
        /// </summary>
        public ITextBuilder AppendNewLine()
        {
            //if (_token != null && !_token.KeepLine) _innerBuilder.Append(' ');
            //else
            //{
            //    _innerBuilder.Append(Environment.NewLine);
            //    if (this.Indent > 0)
            //    {
            //        for (int i = 1; i <= this.Indent; i++) this.AppendNewTab();
            //    }
            //}
            _innerBuilder.Append(Environment.NewLine);
            if (this.Indent > 0)
            {
                for (int i = 1; i <= this.Indent; i++) this.AppendNewTab();
            }
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加回车符
        /// </summary>
        public ITextBuilder AppendNewLine(string value)
        {
            _innerBuilder.Append(value);
            _innerBuilder.AppendLine();
            return this;
        }

        /// <summary>
        /// 将通过处理复合格式字符串（包含零个或零个以上格式项）返回的字符串追加到此实例。每个格式项都替换为形参数组中相应实参的字符串表示形式。
        /// </summary>
        public ITextBuilder AppendFormat(string value, params object[] args)
        {
            _innerBuilder.AppendFormat(value, args);
            return this;
        }

        /// <summary>
        /// 在此实例的结尾追加制表符
        /// </summary>
        public ITextBuilder AppendNewTab()
        {
            _innerBuilder.Append(TextBuilder.TAB);
            return this;
        }

        /// <summary>
        /// 将此实例中所有指定字符串的匹配项替换为其他指定字符串。
        /// </summary>
        public ITextBuilder Replace(string oldValue, string newValue)
        {
            _innerBuilder.Replace(oldValue, newValue);
            return this;
        }

        /// <summary>
        /// 将此值实例转换成 <see cref="string"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _innerBuilder.ToString();
        }
    }
}