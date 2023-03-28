<?xml version="1.0"?>

<!-- Define a new entity so that we can use the HTML &nbsp; -->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#160;">
]>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<!-- <xsl:param name="localeUrl" select="'vault_impact_projection_translations.xml'" />
	<xsl:variable name="localeXml" select="$localeUrl/*" /> -->

	<xsl:template match="/">
		<html>
			<head>
				<link rel="stylesheet" href="styles.css" type="text/css" />
			</head>
			<body>

				<xsl:apply-templates select="/replicationReport"/>

				<script src="js.js" type="text/javascript"></script>

			</body>
		</html>
	</xsl:template>


	<xsl:template match="/replicationReport">

		<header>
			<h1>
				Import vault structure
			</h1>
		</header>

		<article class="main">
			<xsl:apply-templates select="./structure"/>
		</article>

		<footer>
			<button id="cancel" type="button">Cancel</button>
			<button id="import" type="button">Import</button>
		</footer>

	</xsl:template>


	<xsl:template match="/replicationReport/structure">

		<article class="unmapped">
			<xsl:call-template name="vaultStructures">
				<xsl:with-param name="context" select="./unmapped"/>
				<xsl:with-param name="title">Missing items</xsl:with-param>
				<xsl:with-param name="description">These items are missing so will be added during the import.</xsl:with-param>
			</xsl:call-template>
		</article>

		
		<article class="mapped ">
			<xsl:call-template name="vaultStructures">
				<xsl:with-param name="context" select="./mapped"/>
				<xsl:with-param name="title">Existing items</xsl:with-param>
				<xsl:with-param name="description">These items already exist so will not be updated during the import.</xsl:with-param>
			</xsl:call-template>
		</article>

	</xsl:template>

	<xsl:template name="vaultStructures_table">
		<xsl:param name="items" />
		<xsl:param name="title" />
		<xsl:param name="description" />
		<xsl:variable name="total" select="count($items)" />

		<h2><xsl:value-of select="$title" /> (<xsl:value-of select="$total" />)</h2>
		<p class="description"><xsl:value-of select="$description" /></p>

		<table class="vaultStructures" cellspacing="0">
			<thead>
				<th>Item</th>
				<th>Guid</th>
				<th>Aliases</th>
			</thead>
			<tbody>
				<xsl:for-each select="$items">
					<xsl:sort select="name(..)" />
					<xsl:variable name="type" select="name(..)" />
					
					<xsl:variable name="data">
						<xsl:choose>
							<xsl:when test="$type = 'objecttypes' and @realobj =  'true'">Object Type|objectType</xsl:when>
							<xsl:when test="$type = 'objecttypes' and @realobj != 'true'">Value List|valueList</xsl:when>
							<xsl:when test="$type = 'propertydefs'">Property Definition|propertyDef</xsl:when>
							<xsl:when test="$type = 'namedacls'">Named Access Control|namedACL</xsl:when>
							<xsl:when test="$type = 'classes'">Class|objectClass</xsl:when>
							<xsl:when test="$type = 'classgroups'">Class Group|objectClassGroup</xsl:when>
							<xsl:when test="$type = 'workflows'">Workflow|workflow</xsl:when>
							<xsl:when test="$type = 'states'">Workflow State|workflowState</xsl:when>
							<xsl:when test="$type = 'transitions'">Workflow State Transitions|workflowStateTransition</xsl:when>
							<xsl:when test="$type = 'useraccounts'">User Account|userAccount</xsl:when>
							<xsl:when test="$type = 'usergroups'">User Group|userGroup</xsl:when>
							<xsl:when test="$type = 'eventhandlers'">Event Handler|eventHandler</xsl:when>
							<xsl:when test="$type = 'metadatacardconfigurations'">Metadata Card Configuration|metadataCardConfiguration</xsl:when>
							<xsl:when test="$type = 'viewdefs'">View|view</xsl:when>
							<xsl:otherwise>Unknown|unknown</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>

					<xsl:variable name="typeName" select="substring-before($data, '|')" />
					<xsl:variable name="cssClass" select="substring-after($data, '|')" />

					<tr>
						<xsl:attribute name="class"><xsl:value-of select="$cssClass" /></xsl:attribute>
						<xsl:attribute name="title"><xsl:value-of select="concat($typeName, ' - ', @name)" /></xsl:attribute>
						<th><xsl:value-of select="@name" /></th>
						<td><xsl:value-of select="@guid" /></td>
						<td><xsl:value-of select="@aliases" /></td>
					</tr>
				</xsl:for-each>
			</tbody>
		</table>
	</xsl:template>

	<xsl:template name="vaultStructures">
		<xsl:param name="context" />
		<xsl:param name="title" />
		<xsl:param name="description" />
		<xsl:variable name="items" select="$context/*/incoming" />
		<xsl:call-template name="vaultStructures_table">
			<xsl:with-param name="items" select="$items"/>
			<xsl:with-param name="title" select="$title"/>
			<xsl:with-param name="description" select="$description"/>
		</xsl:call-template>
	</xsl:template>

	<!-- <xsl:template name="vaultStructuresOld">
		<xsl:param name="context" />
		<xsl:param name="title" />
		<xsl:param name="description" />
		<xsl:variable name="objectTypes" select="$context/objecttypes/incoming[@realobj='true']" />
		<xsl:variable name="classGroups" select="$context/classgroups/incoming" />
		<xsl:variable name="classes" select="$context/classes/incoming" />
		<xsl:variable name="valueLists" select="$context/objecttypes/incoming[@realobj='false']" />
		<xsl:variable name="propertyDefinitions" select="$context/propertydefs/incoming" />
		<xsl:variable name="workflows" select="$context/workflows/incoming" />
		<xsl:variable name="workflowStates" select="$context/states/incoming" />
		<xsl:variable name="transitions" select="$context/transitions/incoming" />
		<xsl:variable name="namedAccessControlLists" select="$context/namedacls/incoming" />
		<xsl:variable name="userGroups" select="$context/usergroups/incoming" />
		<xsl:variable name="users" select="$context/useraccounts/incoming" />

		<xsl:variable name="vaultStructureCount" select="count($objectTypes) + count($classGroups) + count($classes) + count($valueLists) + count($propertyDefinitions) + count($workflows) + count($workflowStates) + count($transitions)" />
		<xsl:variable name="securityCount" select="count($namedAccessControlLists) + count($userGroups) + count($users)" />
		<xsl:variable name="functionalityCount">0</xsl:variable>
		<xsl:variable name="usabilityCount">0</xsl:variable>
		<xsl:variable name="total" select="count($context/*/incoming)" />

		<h2><xsl:value-of select="$title" /> (<xsl:value-of select="$total" />)</h2>
		<p class="description"><xsl:value-of select="$description" /></p>

		<div class="vaultStructures">

			<xsl:if test="$vaultStructureCount &gt; 0">

				<h3>Vault structure <span class="count">(<xsl:value-of select="$vaultStructureCount" />)</span></h3>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Object types</xsl:with-param>
					<xsl:with-param name="items" select="$objectTypes"/>
					<xsl:with-param name="class">objecttypes</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Class Groups</xsl:with-param>
					<xsl:with-param name="items" select="$classGroups"/>
					<xsl:with-param name="class">classgroups</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Classes</xsl:with-param>
					<xsl:with-param name="items" select="$classes"/>
					<xsl:with-param name="class">classes</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Value lists</xsl:with-param>
					<xsl:with-param name="items" select="$valueLists"/>
					<xsl:with-param name="class">valuelists</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Property definitions</xsl:with-param>
					<xsl:with-param name="items" select="$propertyDefinitions"/>
					<xsl:with-param name="class">propertydefs</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Workflows</xsl:with-param>
					<xsl:with-param name="items" select="$workflows"/>
					<xsl:with-param name="class">workflows</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Workflow states</xsl:with-param>
					<xsl:with-param name="items" select="$workflowStates"/>
					<xsl:with-param name="class">states</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">State transitions</xsl:with-param>
					<xsl:with-param name="items" select="$transitions"/>
					<xsl:with-param name="class">transitions</xsl:with-param>
				</xsl:call-template>

			</xsl:if>

			<xsl:if test="$securityCount &gt; 0">

				<h3>Security and permissions <span class="count">(<xsl:value-of select="$securityCount" />)</span></h3>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Named Access Control Lists</xsl:with-param>
					<xsl:with-param name="items" select="$namedAccessControlLists"/>
					<xsl:with-param name="class">nacls</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">User groups</xsl:with-param>
					<xsl:with-param name="items" select="$userGroups"/>
					<xsl:with-param name="class">usergroups</xsl:with-param>
				</xsl:call-template>

				<xsl:call-template name="valueStructures_list">
					<xsl:with-param name="title">Users</xsl:with-param>
					<xsl:with-param name="items" select="$users"/>
					<xsl:with-param name="class">users</xsl:with-param>
				</xsl:call-template>

			</xsl:if>

			<xsl:if test="$functionalityCount &gt; 0">

				<h3>Functionality</h3>
				

			</xsl:if>

			<xsl:if test="$usabilityCount &gt; 0">

				<h3>User experience</h3>


			</xsl:if>

		</div>

	</xsl:template>

	<xsl:template name="valueStructures_list">
		<xsl:param name="title" />
		<xsl:param name="items" />
		<xsl:param name="class" />

		<xsl:if test="count($items) &gt; 0">
			<article>
			<xsl:attribute name="class"><xsl:value-of select="concat('vaultstructure ', $class)" /></xsl:attribute>

				<h4><xsl:value-of select="$title" /> (<xsl:value-of select="count($items)" />)</h4>
				<xsl:for-each select="$items">	
					<ul>
						<li><xsl:value-of select="@name" /></li>
					</ul>
				</xsl:for-each>
			</article>
		</xsl:if>
		

	</xsl:template> -->

</xsl:stylesheet>


