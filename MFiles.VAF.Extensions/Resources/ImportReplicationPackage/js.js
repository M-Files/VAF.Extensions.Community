document.getElementById("cancel").addEventListener("click", function(){ alert("cancel")});
document.getElementById("import").addEventListener("click", function(){ alert("import")});


function addCollapseExpandHandlers()
{
	var addHandlers = function(a)
	{
		for(var i=0; i != a.length; i++)
		{
			var e = a[i];
			e.addEventListener("click", function()
			{
				this.classList.toggle("collapsed");
			});
			e.classList.add("collapsed");
		}
	};
	addHandlers(document.getElementsByTagName("h2"));
	addHandlers(document.getElementsByTagName("h3"));
}
addCollapseExpandHandlers();