using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets {
	public class EntityPool<T> {
		private Queue<T> PoolQueue = new Queue<T>();
		private uint PoolSize = 0;

		public EntityPool(uint size) {
			PoolSize = size;
		}
	}
}
