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
				<xsl:with-param name="description">These items already exist so may be updated during the import.</xsl:with-param>
			</xsl:call-template>
		</article>

		
		<article class="skipped ">
			<xsl:call-template name="vaultStructures">
				<xsl:with-param name="context" select="./skipped"/>
				<xsl:with-param name="title">Skipped items</xsl:with-param>
				<xsl:with-param name="description">These items will be skipped during the import.</xsl:with-param>
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

		<xsl:if test="$total &gt; 0">
			<table class="vaultStructures" cellspacing="0">
				<thead>
					<tr>
						<th>Item</th>
						<th>&nbsp;</th>
						<th>Id</th>
						<th>Guid</th>
						<th>Aliases</th>
					</tr>
				</thead>
				<tbody>
					<xsl:for-each select="$items">
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

						<xsl:variable name="local" select="./local" />

						<tr>
							<xsl:attribute name="class"><xsl:value-of select="concat('incoming local-count-', count($local), ' ', $cssClass)" /></xsl:attribute>
							<xsl:attribute name="title"><xsl:value-of select="concat($typeName, ' - ', @name)" /></xsl:attribute>
							<th>
								<xsl:if test="count($local) &gt; 0">
									<xsl:attribute name="rowspan">2</xsl:attribute>
								</xsl:if>
								<xsl:value-of select="@name" />
							</th>
							<td class="heading"><h3>Incoming</h3></td>
							<td class="number"><xsl:value-of select="@id" /></td>
							<td class="guid"><xsl:value-of select="@guid" /></td>
							<td><xsl:value-of select="@aliases" /></td>
						</tr>
						<xsl:if test="count($local) &gt; 0">
							<tr>
								<xsl:attribute name="class"><xsl:value-of select="concat('local ', $cssClass)" /></xsl:attribute>
								<xsl:attribute name="title"><xsl:value-of select="concat($typeName, ' - ', @name)" /></xsl:attribute>
								<td class="heading"><h3>Local</h3></td>
								<td class="number"><xsl:value-of select="$local/@id" /></td>
								<td class="guid"><xsl:value-of select="$local/@guid" /></td>
								<td><xsl:value-of select="$local/@aliases" /></td>
							</tr>
						</xsl:if>
					</xsl:for-each>
				</tbody>
			</table>

		</xsl:if>

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

</xsl:stylesheet>


