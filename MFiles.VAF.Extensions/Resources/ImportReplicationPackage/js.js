function OnNewDashboard(dashboard)
{
	// Close == cancel.
	document.getElementById("cancel").addEventListener
	(
		"click",
		 function()
		 { 
			dashboard.Window.Close();
		 }
	);

	var importMethod = %IMPORT_METHOD%;
	if(null != importMethod)
	{
		document.getElementById("import").addEventListener
		(
			"click", 
			function()
			{
				importMethod.source.read;
				var params = importMethod.params;
				this.dashboard
					.Vault
					.ExtensionMethodOperations
					.ExecuteVaultExtensionMethod
					(
						importMethod.method,
						JSON.stringify( params )
					);


					dashboard.Window.Close();
			}
		);
	}
	else
	{
		document.getElementById("import").classList.add("hidden");
	}

	document.body.classList.add("showFooter");
}


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