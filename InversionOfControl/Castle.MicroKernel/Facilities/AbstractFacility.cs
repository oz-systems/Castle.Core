// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.MicroKernel.Facilities
{
	using System;

	using Castle.Model.Configuration;

	/// <summary>
	/// Base class for facilities. 
	/// </summary>
	public abstract class AbstractFacility : IFacility, IDisposable
	{
		private IKernel _kernel;
		private IConfiguration _facilityConfig;

		public IKernel Kernel
		{
			get { return _kernel; }
		}

		public IConfiguration FacilityConfig
		{
			get { return _facilityConfig; }
		}

		protected abstract void Init();

		#region IFacility Members

		public void Init(IKernel kernel, IConfiguration facilityConfig)
		{
			_kernel = kernel;
			_facilityConfig = facilityConfig;

			Init();
		}

		public void Terminate()
		{
			Dispose();
			
			_kernel = null;
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
		}

		#endregion
	}
}
