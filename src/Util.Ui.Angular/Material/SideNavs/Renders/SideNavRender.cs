﻿using Util.Ui.Builders;
using Util.Ui.Configs;
using Util.Ui.Material.Enums;
using Util.Ui.Material.SideNavs.Builders;
using Util.Ui.Renders;

namespace Util.Ui.Material.SideNavs.Renders {
    /// <summary>
    /// 侧边栏导航渲染器
    /// </summary>
    public class SideNavRender : RenderBase {
        /// <summary>
        /// 配置
        /// </summary>
        private readonly IConfig _config;

        /// <summary>
        /// 初始化侧边栏导航渲染器
        /// </summary>
        /// <param name="config">配置</param>
        public SideNavRender( IConfig config ) : base( config ) {
            _config = config;
        }

        /// <summary>
        /// 获取标签生成器
        /// </summary>
        protected override TagBuilder GetTagBuilder() {
            var builder = new SideNavBuilder();
            Config( builder );
            return builder;
        }

        /// <summary>
        /// 配置
        /// </summary>
        protected void Config( TagBuilder builder ) {
            ConfigId( builder );
            ConfigPosition( builder );
            ConfigOpened( builder );
            ConfigContent( builder );
        }

        /// <summary>
        /// 配置位置
        /// </summary>
        private void ConfigPosition( TagBuilder builder ) {
            if ( _config.Contains( UiConst.Position ) == false )
                return;
            builder.AddAttribute( UiConst.Position, _config.GetValue<XPosition>( UiConst.Position ) == XPosition.Left ? "start": "end" );
        }

        /// <summary>
        /// 配置是否打开
        /// </summary>
        private void ConfigOpened( TagBuilder builder ) {
            builder.AddAttribute( UiConst.Opened, _config.GetBoolValue( UiConst.Opened ) );
        }

        /// <summary>
        /// 配置内容
        /// </summary>
        private void ConfigContent( TagBuilder builder ) {
            if( _config.Content == null )
                return;
            builder.SetContent( _config.Content );
        }
    }
}