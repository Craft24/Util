﻿using System;
using System.Threading.Tasks;
using Util.Datas.Ef;
using Util.Datas.Tests.Samples.Datas.SqlServer.UnitOfWorks;
using Util.Datas.Tests.Samples.Domains.Models;
using Util.Datas.Tests.Samples.Domains.Repositories;
using Util.Datas.Tests.SqlServer.Confis;
using Util.Datas.Tests.XUnitHelpers;
using Util.Exceptions;
using Util.Helpers;
using Xunit;

namespace Util.Datas.Tests.SqlServer.Tests {
    /// <summary>
    /// 订单仓储测试
    /// </summary>
    public class OrderRepositoryTest : IDisposable {
        /// <summary>
        /// 容器
        /// </summary>
        private readonly Util.DependencyInjection.IContainer _container;
        /// <summary>
        /// 工作单元
        /// </summary>
        private readonly ISqlServerUnitOfWork _unitOfWork;
        /// <summary>
        /// 订单仓储
        /// </summary>
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// 测试初始化
        /// </summary>
        public OrderRepositoryTest() {
            _container = Ioc.CreateContainer( new IocConfig() );
            _unitOfWork = _container.Create<ISqlServerUnitOfWork>();
            _orderRepository = _container.Create<IOrderRepository>();
        }

        /// <summary>
        /// 测试清理
        /// </summary>
        public void Dispose() {
            _container.Dispose();
        }

        /// <summary>
        /// 测试添加
        /// </summary>
        [Fact]
        public void TestAdd() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Equal( id, result.Id );
        }

        /// <summary>
        /// 测试异步添加
        /// </summary>
        [Fact]
        public async Task TestAddAsync() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.CommitAsync();

            var result = await _orderRepository.SingleAsync( t => t.Id == id );
            Assert.Equal( id, result.Id );
        }

        /// <summary>
        /// 测试更新 - 修改方式1：先从仓储中查找出来，直接修改对象属性，提交工作单元
        /// </summary>
        [Fact]
        public void TestUpdate_1() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            order = _orderRepository.Find( id );
            order.Code = "B";
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Equal( "B", result.Code );
        }

        /// <summary>
        /// 测试更新 - 修改方式2：创建出修改对象，调用仓储的Update，提交工作单元
        /// 应用场景：通过Http Put方法传入DTO,该DTO包含待修改实体的全部属性，将该DTO转成待更新实体
        /// 注意：创建的更新实体必须包含全部数据，否则导致数据丢失
        /// </summary>
        [Fact]
        public void TestUpdate_2() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            order = new Order( id ) { Name = "Name", Code = "B", Version = order.Version };
            _orderRepository.Update( order );
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Equal( "B", result.Code );
        }

        /// <summary>
        /// 测试更新并发 - 使用修改方式2,Version属性设置为无效
        /// </summary>
        [Fact]
        public void TestUpdate_Concurrency() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            AssertHelper.Throws<ConcurrencyException>( () => {
                var order2 = new Order( id ) { Name = "Name", Code = "B", Version = Guid.NewGuid().ToByteArray() };
                _orderRepository.Update( order2 );
                _unitOfWork.Commit();
            } );
        }

        /// <summary>
        /// 测试异步更新
        /// </summary>
        [Fact]
        public async Task TestUpdateAsync() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.CommitAsync();
            _unitOfWork.ClearCache();

            var oldEntity = await _orderRepository.FindAsync( id );
            order = new Order( id ) { Name = "Name", Code = "B", Version = oldEntity.Version };
            await _orderRepository.UpdateAsync( order );
            await _unitOfWork.CommitAsync();

            var result = await _orderRepository.SingleAsync( t => t.Id == id );
            Assert.Equal( "B", result.Code );
        }

        /// <summary>
        /// 测试删除 - 通过实体标识删除
        /// </summary>
        [Fact]
        public void TestRemove_Key() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            _orderRepository.Remove( id );
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Null( result );
        }

        /// <summary>
        /// 测试异步删除 - 通过实体标识删除
        /// </summary>
        [Fact]
        public async Task TestRemoveAsync_Key() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.CommitAsync();
            _unitOfWork.ClearCache();

            await _orderRepository.RemoveAsync( id );
            await _unitOfWork.CommitAsync();

            var result = await _orderRepository.SingleAsync( t => t.Id == id );
            Assert.Null( result );
        }

        /// <summary>
        /// 测试删除 - 通过实体删除 - Find查出来，Remove
        /// </summary>
        [Fact]
        public void TestRemove_Entity() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            order = _orderRepository.Find( id );
            Assert.NotNull( order );
            _orderRepository.Remove( order );
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Null( result );
        }

        /// <summary>
        /// 测试删除 - 通过实体删除 - 创建待删除实体,Remove
        /// </summary>
        [Fact]
        public void TestRemove_Entity_2() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            _orderRepository.Add( order );
            _unitOfWork.Commit();
            _unitOfWork.ClearCache();

            _orderRepository.Find( id );
            order = new Order( id ) { Name = "Name", Code = "Code", Version = order.Version };
            _orderRepository.Remove( order );
            _unitOfWork.Commit();

            var result = _orderRepository.Single( t => t.Id == id );
            Assert.Null( result );
        }

        /// <summary>
        /// 测试异步删除 - 通过实体删除
        /// </summary>
        [Fact]
        public async Task TestRemoveAsync_Entity() {
            Guid id = Guid.NewGuid();
            var order = new Order( id ) { Name = "Name", Code = "Code" };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.CommitAsync();
            _unitOfWork.ClearCache();

            order = await _orderRepository.FindAsync( id );
            Assert.NotNull( order );
            await _orderRepository.RemoveAsync( order );
            await _unitOfWork.CommitAsync();

            var result = await _orderRepository.SingleAsync( t => t.Id == id );
            Assert.Null( result );
        }
    }
}
