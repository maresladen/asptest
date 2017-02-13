
function TinyEditor(el,opt){
	var DefaultOpt = {
		index : '1'
	}
	ace.require("ace/ext/language_tools");
	opt = utils.mixin(DefaultOpt,opt,true)
	this.init(el,opt)
}

/**
 * 同步加载js脚本
 * @param id   需要设置的<script>标签的id
 * @param url   js文件的相对路径或绝对路径
 * @return {Boolean}   返回是否加载成功，true代表成功，false代表失败
 */
function loadJS(url,headNode,scriptNode,theFrame){
    var  xmlHttp = null;
    if(window.ActiveXObject)//IE
    {
        try {
            //IE6以及以后版本中可以使用
            xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
        }
        catch (e) {
            //IE5.5以及以后版本可以使用
            xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
        }
    }
    else if(theFrame.window.XMLHttpRequest)//Firefox，Opera 8.0+，Safari，Chrome
    {
        xmlHttp =new theFrame.window.XMLHttpRequest();
    }
    //采用同步加载
    xmlHttp.open("GET",url,false);
    //发送同步请求，如果浏览器为Chrome或Opera，必须发布后才能运行，不然会报错
    xmlHttp.send(null);
    //4代表数据发送完毕
    if ( xmlHttp.readyState == 4 )
    {
        //0为访问的本地，200到300代表访问服务器成功，304代表没做修改访问的是缓存
        if((xmlHttp.status >= 200 && xmlHttp.status <300) || xmlHttp.status == 0 || xmlHttp.status == 304)
        {
            try{
                //IE8以及以下不支持这种方式，需要通过text属性来设置
                scriptNode.appendChild(theFrame.document.createTextNode(xmlHttp.responseText));
            }
            catch (ex){
                scriptNode.text = xmlHttp.responseText;
            }
            headNode.appendChild( scriptNode );
            return true;
        }
        else
        {
            return false;
        }
    }
    else
    {
        return false;
    }
}

//-- 基本方法封装
var utils = {
	op	: function(){
		var op = Object.prototype,
			ap	= Array.prototype;
		return {
			ostring		: op.toString,
			hasOwn		: op.hasOwnProperty,
		}
	},
	eachProp : function(obj, func){
		var prop;
		for(prop in obj){
			if(this.hasProp(obj, prop)){
				if(func(obj[prop], prop)){
					break;
				}
			}
		}
	},
	hasProp : function(obj, prop){
		return this.op().hasOwn.call(obj, prop)
	},
	mixin : function(target, source, force, deepStringMixin) {
		if(source){
			this.eachProp(source, function(value, prop){
				if (force || !this.hasProp(target, prop)) {
					if (deepStringMixin && _.isObject(value) && value &&
						!_.isArray(value) && !_.isFunction(value) &&
						!(value instanceof RegExp)){
						if(!target[prop]){
							target[prop] = {};
						}
						mixin(target[prop], value, force, deepStringMixin);
					}else{
						target[prop] = value;
					}
				}
			})
		}
		return target;
	},
	id:function(id){
		return document.getElementById(id);
	},

	//-- 缓存读写器
	store : {
		set:function(key,value){
			localStorage.setItem(key,value);
		},
		get:function(key) {
			return localStorage.getItem(key);
		}
	},

	//-- 读缓存里的代码
	isStore : function(key,editor){
		if(!!utils.store.get(key)){
			var result = utils.store.get(key);
		}else{
			var result = editor.getValue();
		}
		return result;
	},
}


var buffer = {
	//-- �������
	tabPlugin:document.getElementById('tabPlugin'),
	tabControl:document.getElementById('tabControls'),
	tabContainer:this.tabPlugin.getElementsByClassName('tabContainer'),

	//-- �����
	htmlEditor : ace.edit("html"),
	cssEditor : ace.edit("css"),
	jsEditor : ace.edit("js"),

	editorArr : ['htmlEditor','cssEditor','jsEditor'],

	//-- �����
	setOptions:{
		enableBasicAutocompletion: true,
		enableSnippets: true,
		enableLiveAutocompletion: true
	},
	// //-- �����
	// setTheme: utils.store.get('theme')||'ace/theme/monokai',

	// Themes:['ambiance.css','chaos.css','chrome.css','clouds.css','clouds_midnight.css','cobalt.css','crimson_editor.css','dawn.css','dreamweaver.css','eclipse.css','github.css','idle_fingers.css','iplastic.css','katzenmilch.css','kr_theme.css','kuroir.css','merbivore.css','merbivore_soft.css','mono_industrial.css','monokai.css','pastel_on_dark.css','solarized_dark.css','solarized_light.css','sqlserver.css','terminal.css','textmate.css','tomorrow.css','tomorrow_night.css','tomorrow_night_blue.css','tomorrow_night_bright.css','tomorrow_night_eighties.css','twilight.css','vibrant_ink.css','xcode.css']
}



TinyEditor.prototype = {

	init : function(el,opt){
		var self = this
		if(utils.store.get('dockMode')=='true'){
			utils.dockMode = false;
			self.dockMode();
		}

		//-- 初始化适应最外层id的高
		for(var i = 0,len=buffer.tabContainer.length;i<len;i++){
			buffer.tabContainer[i].style.height= (buffer.tabPlugin.offsetHeight-40)+'px';
		}

		this.tab(opt.index);

		//-- 遍历选择主题
		// var options = ''
		// for(var j = 0,jlen = buffer.Themes.length;j<jlen;j++){
		// 	var theme = buffer.Themes[j].replace('.css','');
		// 	options+='<option value="'+theme+'">'+theme+'</option>'
		// }
		// utils.id('theme').innerHTML=options
		// utils.id('theme').value=buffer.setTheme.replace('ace/theme/','')
		// utils.id('theme').onchange = function(){
		// 	self.setTheme('ace/theme/'+this.value)
		// 	utils.store.set('theme','ace/theme/'+this.value)
		// }


		//-- 初始化设置编辑器
		buffer.htmlEditor.session.setMode("ace/mode/html");
		// buffer.htmlEditor.setTheme(buffer.setTheme);
		buffer.htmlEditor.setOptions(buffer.setOptions);
		buffer.htmlEditor.setValue(utils.isStore('html',buffer.htmlEditor));
		buffer.htmlEditor.moveCursorTo(0,0);
		buffer.htmlEditor.resize(true)

		buffer.cssEditor.session.setMode("ace/mode/css");
		// buffer.cssEditor.setTheme(buffer.setTheme);
		buffer.cssEditor.setOptions(buffer.setOptions);
		buffer.cssEditor.setValue(utils.isStore('css',buffer.cssEditor));
		buffer.cssEditor.moveCursorTo(0,0);
		buffer.cssEditor.resize(true)

		buffer.jsEditor.session.setMode("ace/mode/javascript");
		// buffer.jsEditor.setTheme(buffer.setTheme);
		buffer.jsEditor.setOptions(buffer.setOptions);
		buffer.jsEditor.setValue(utils.isStore('js',buffer.jsEditor));
		buffer.jsEditor.moveCursorTo(0,0);
		buffer.jsEditor.resize(true)

		//-- ctrl+enter运行
		document.onkeydown= function(e) { e=e||window.event;
			kc=e.keyCode||e.charCode;
			if ((kc == 13&&e.ctrlKey)) {
				self.run()
			}
		}

	},

	//-- tab组建，建议自己写
	tab : function(index){
		for(var i = 0,len=buffer.tabControl.getElementsByTagName('li').length;i<len;i++ ){
			buffer.tabContainer[i].style.zIndex='0';
			buffer.tabContainer[i].style.opacity='.3';
			buffer.tabControl.getElementsByTagName('li')[i].classList.remove('active')
		}
		buffer.tabControl.getElementsByTagName('li')[index-1].classList.add('active')
		buffer.tabContainer[index-1].style.zIndex='1';
		buffer.tabContainer[index-1].style.opacity='1';
		buffer.editorArr[index-1]&&buffer[buffer.editorArr[index-1]].focus()


		if(!!utils.dockMode){
			document.getElementById('dockIframe').style.zIndex='1';
			document.getElementById('dockIframe').style.opacity='1';
			this.splitResize(index-1)
			buffer.tabContainer[index-1].style.width=(buffer.tabPlugin.offsetWidth-utils.id('dockIframe').offsetWidth)+'px'
		}

	},

	// //-- 设置主题
	// setTheme: function(theme){
	// 	buffer.htmlEditor.setTheme(theme);
	// 	buffer.cssEditor.setTheme(theme);
	// 	buffer.jsEditor.setTheme(theme);
	// },

	//-- dock边栏模式
	dockMode : function(){
		if(!!utils.dockMode){   //-- 取消dock模式
			if(utils.liveMode == true){
				this.liveMode(); //-- 当取消dock模式，live模式也取消 
			}
			utils.id('liveButton').classList.add('hide')
			utils.dockMode = false;
			document.getElementById('dockButton').classList.remove('active')
			document.getElementById('resultTab').classList.remove('hide')
			document.getElementById('dockIframe').className='tabContainer'
			document.getElementById('dockIframe').style.zIndex='0';
			document.getElementById('dockIframe').style.opacity='.3';
			for(var i = 0,len=buffer.tabControl.getElementsByTagName('li').length;i<len;i++ ){
				buffer.tabContainer[i].style.width='100%'
			}
		}else{				//-- 开启dock模式
			utils.dockMode = true;
			utils.id('dockButton').classList.add('active')
			utils.id('liveButton').classList.remove('hide')

			if(document.getElementById('resultTab').className.match('active')){
				this.tab(1)
			}
			document.getElementById('resultTab').classList.add('hide')
			var currentContainer = 0;
			for(var i = 0,len=buffer.tabControl.getElementsByTagName('li').length;i<len;i++ ){
				if(buffer.tabControl.getElementsByTagName('li')[i].className=='active'){
					currentContainer = i
				}
			}
			this.splitResize(currentContainer)
			buffer.tabContainer[currentContainer].style.width=buffer.tabPlugin.offsetWidth-400 + 'px'
			document.getElementById('dockIframe').className='tabContainer dockMode'
			document.getElementById('dockIframe').style.height=(buffer.tabPlugin.offsetHeight-40)+'px';
			document.getElementById('dockIframe').style.width='400px';
			document.getElementById('dockIframe').style.opacity='1';
		}
	},

	//-- 边栏的分割线调整
	splitResize : function(i){
		document.getElementById('splitLine').onmousedown=function(e){
			var ox = e.clientX;
			var dockIframeW = document.getElementById('dockIframe').offsetWidth;
			var dockContainerW = buffer.tabContainer[1].offsetWidth;
			document.onmousemove = function(e) {
				e.preventDefault()
				buffer.htmlEditor.resize(true)
				var x = e.clientX - ox;
				var resizeW = dockIframeW - x;
				buffer.tabContainer[i].style.width= (dockContainerW + x) + 'px'
				document.getElementById('dockIframe').style.width = resizeW + 'px'
			}
			document.onmouseup=function(e){
				document.onmousemove=null;
			}
		}
	},

	//-- 实时运行模式，当编辑器发生change事件触发
	liveMode:function(){
		var self = this;
		if(!!utils.dockMode) {
			if (!!utils.liveMode) {   	//-- 取消live模式
				utils.liveMode = false;
				document.getElementById('liveButton').classList.remove('active')
			} else { 					//--开始live模式
				utils.liveMode = true;
				document.getElementById('liveButton').classList.add('active')
			}
			buffer.htmlEditor.on('change', function (e) {
				!!utils.liveMode&&self.run()
			})
			buffer.cssEditor.on('change', function (e) {
				!!utils.liveMode&&self.run()
			})
			buffer.jsEditor.on('change', function (e) {
				!!utils.liveMode&&self.run()
			})
		}
	},

	//-- 全屏模式
	fullScreen:function(){
		if(document.getElementById('tabPlugin').className.match('full')){
			document.getElementById('tabPlugin').classList.remove('full')
		}else{
			document.getElementById('tabPlugin').classList.add('full')
		}
		for(var i = 0,len=buffer.tabContainer.length;i<len;i++){
			buffer.tabContainer[i].style.height= (buffer.tabPlugin.offsetHeight-40)+'px';
		}
	},

	//-- 保存代码记录
	save:function(){
		utils.store.set('html',buffer.htmlEditor.getValue())
		utils.store.set('css',buffer.cssEditor.getValue())
		utils.store.set('js',buffer.jsEditor.getValue())
		console.log(utils.dockMode)
		utils.store.set('liveMode',utils.liveMode)
		utils.store.set('dockMode',utils.dockMode)
	},

	//-- 运行并跳转到result面板
	run:function(){
		!utils.dockMode&&this.tab(4)
		var html=buffer.htmlEditor.getValue();
		var css = '<style>'+buffer.cssEditor.getValue()+'</style>';
		var js = buffer.jsEditor.getValue();

		var cssDep ='';

		for(var i =0;i<editVue.feaDepends["text/css"].length;i++){

			cssDep += '<link rel="stylesheet" href="'+ editVue.feaDepends["text/css"][i].depSrc +'">\n';
		}


		var iframe = document.getElementsByTagName('iframe')[0].contentWindow;
        var oHead = iframe.document.getElementsByTagName('head')[0]


		var sList = oHead.getElementsByTagName('script');
		if(sList.length == 0){
			//这里要做动态的添加的，通过API接口请求去获得要引入多少js和css
			for (var i = 0; i < editVue.feaDepends["text/javascript"].length; i++) {
				var oScript = iframe.document.createElement("script");
				oScript.type = "text/javascript";
				oScript.async = false;
				loadJS(editVue.feaDepends["text/javascript"][i].depSrc, oHead, oScript, iframe);
			}
		}

		iframe.document.getElementsByTagName('body')[0].innerHTML=(cssDep + html+css)


		window.frames['iframe'].eval(js)

	}

}
