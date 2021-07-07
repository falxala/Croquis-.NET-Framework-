using Croquis_.NET_Framework_.Properties;

namespace Croquis_.NET_Framework_
{
    class ResourceService
    {
        #region singleton members

        private static readonly ResourceService _current = new ResourceService();
        public static ResourceService Current
        {
            get { return _current; }
        }

        #endregion

        private readonly Resources _resources = new Resources();

        /// <summary>
        /// 多言語化されたリソースを取得します。
        /// </summary>
        public Resources Resources
        {
            get { return _resources; }
        }
    }
}
