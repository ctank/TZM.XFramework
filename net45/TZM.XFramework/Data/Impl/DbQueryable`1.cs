﻿
using System.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TZM.XFramework.Data
{
    /// <summary>
    /// 数据查询表达对象
    /// </summary>
    public class DbQueryable<TElement> : DbQueryable, IDbQueryable<TElement>
    {
        private ReadOnlyCollection<DbExpression> _collection = null;

        /// <summary>
        /// 查询表达式
        /// </summary>
        public override ReadOnlyCollection<DbExpression> DbExpressions
        {
            get { return _collection; }
        }

        /// <summary>
        /// 实例化类<see cref="DbQueryable"/>的新实例
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public DbQueryable(IDbContext context, IList<DbExpression> collection)
        {
            this.DbContext = context;
            this._collection = new ReadOnlyCollection<DbExpression>(collection != null ? collection : new List<DbExpression>(0));
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        public IDbQueryable<TResult> CreateQuery<TResult>(DbExpressionType dbExpressionType, System.Linq.Expressions.Expression expression = null)
        {
            return this.CreateQuery<TResult>(new DbExpression(dbExpressionType, expression));
        }

        /// <summary>
        /// 创建查询
        /// </summary>
        public IDbQueryable<TResult> CreateQuery<TResult>(DbExpression dbExpression = null)
        {
            List<DbExpression> collection = new List<DbExpression>(this.DbExpressions.Count + (dbExpression != null ? 1 : 0));
            collection.AddRange(this.DbExpressions);
            if (dbExpression != null) collection.Add(dbExpression);

            IDbQueryable<TResult> query = new DbQueryable<TResult>(this.DbContext, collection);
            return query;
        }

        /// <summary>
        /// 解析成 SQL 命令
        /// </summary>
        /// <param name="indent">缩进</param>
        /// <param name="isOuter">是否最外层，内层查询不需要结束符(;)</param>
        /// <param name="parameters">已存在的参数列表</param>
        /// <returns></returns>
        public override DbCommandDefinition Resolve(int indent = 0, bool isOuter = true, List<IDbDataParameter> parameters = null)
        {
            var cmd = this.Provider.Resolve(this, indent, isOuter, parameters);
            return cmd;
        }

        /// <summary>
        /// 解析查询语义
        /// </summary>
        public override IDbQueryableInfo Parse(int startIndex = 0)
        {
            return DbQueryParser.Parse(this);
        }

        /// <summary>
        /// 字符串表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var newQuery = this.CreateQuery<TElement>();
            newQuery.Parameterized = false;
            var cmd = newQuery.Resolve(0, true, null);
            return cmd.CommandText;
        }
    }
}