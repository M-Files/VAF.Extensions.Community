<?xml version="1.0"?>

<!-- Define a new entity so that we can use the HTML &nbsp; -->
<!DOCTYPE xsl:stylesheet [
	<!ENTITY nbsp "&#160;">
]>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:callbackObj="urn:callbackObj">

	<!-- *** Declare global variables. -->

	<xsl:param name="localeUrl" select="'vault_impact_projection_translations.xml'"/>
	<xsl:variable name="localeXml" select="document($localeUrl)/*" />
	<xsl:variable name="colspanfull" select="'8'" />
	<xsl:variable name="colspanhalf" select="'4'" />

	<!-- *** Header and style sheet definitions. -->

	<xsl:template match="/">
		<html>
			<!-- HTML head part. -->
			<head>
				<!-- Define inline css stylesheet. -->
				<style type="text/css">

					body,html
					{
					background-color : white;
					font: 11pt Segoe UI, Tahoma, Arial, sans-serif;
					margin:1em;
					margin-bottom:0px;
					}

					h1
					{
					font-size:1.75em;
					}

					h2
					{
					font-size:1.25em;
					}

					h3
					{
					font-size:1.25em;
					margin-top:25px;
					margin-bottom:10px;
					margin-left:20px;
					}

					.details_table, .details_table th
					{
					font-size: 11px;
					margin-left:2px;
					margin-bottom: 0.5cm;
					vertical-align: top;
					}

					.details_table
					{
					border-style:solid;
					border-width:1px;
					border-color: lightgrey;
					border-collapse:collapse;
					width:100%;
					}

					.details_table th
					{
					border-collapse:collapse;
					border-spacing: 10px 5px;
					text-align:left;
					/*color: white;*/
					background-color:white;
					font-weight:bold;
					padding:5px;
					}

					td
					{
					padding:5px;
					vertical-align:top;
					}

					.cell_changed
					{
					text-align:center;
					font-weight:bold;
					font-size: 11px;
					}

					.even_row
					{
					background-color:#F2F2F2;
					}

					.table_subtitle {
					background-color: #B2B2B2;
					font-weight: bold;
					color: white;
					}

					.details_section_table
					{
					font-size: 11px;
					margin-left:20px;
					}

					.details_section
					{
					font-size: 11px;
					margin-left:20px;
					margin-bottom:10px;
					}

					.details_section_row_title
					{
					font-weight:bold;
					}

					.samevalue
					{
					font-weight:normal;
					}

					.differentvalue_text
					{
					background-color: #fff2a8;
					}

					.highlightedheader
					{
					text-transform:uppercase;
					}

					.divider_column
					{
					border-right-style: solid;
					border-right-width: 1px;
					border-right-color: lightgrey;
					}

					.warning_note_prefix
					{
					color: red;
					font-weight:bold;
					}

					.warning_note
					{
					color: red;
					font-weight:normal;
					}

					.clickable
					{
					text-decoration: underline;
					cursor: pointer;
					}

					td {
					max-width: 200px;
					word-wrap: break-word;
					}

					td:nth-of-type(8) {
					border-left: 1px solid lightgray;
					}

					.details_table > tbody > tr:nth-of-type(2) > th:nth-of-type(4) {
					border-right: 1px solid lightgray;
					}

					.details_table > tbody > tr:nth-of-type(2) > th:nth-of-type(7) {
					border-right: 1px solid lightgray;
					}

					.border_left {
					border-left: 1px solid lightgray;
					}

				</style>
			</head>
			<body>

				<!-- Apply templates. -->
				<xsl:apply-templates select="/replicationReport"/>

			</body>
		</html>
	</xsl:template>


	<!-- *** Template: print_table_local_and_incoming -->

	<xsl:template name="print_table_local_and_incoming">
		<xsl:param name="path_to_table_element"/>
		<xsl:param name="mode"/>
		<xsl:param name="is_full_element_str"/>
		<xsl:param name="is_changed_str"/>

		<xsl:if test="count($path_to_table_element//incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str)]) &gt; 0">

			<table class="details_table">

				<!-- Print table headers columns depending on mode -->
				<xsl:choose>

					<!-- Mode: Mapped. Print local and incoming data columns. -->
					<xsl:when test="$mode = 'mode_mapped'">
						<tr>

							<th class="highlightedheader">
								<xsl:attribute name="colspan">
									<xsl:value-of select="$colspanhalf"/>
								</xsl:attribute>
								<xsl:value-of select="$localeXml/translation[@id='TableHeaderText_CurrentMetadataElements']"/>
							</th>

							<th class="highlightedheader divider_column border_left" colspan="3">
								<xsl:choose>
									<xsl:when test="$is_full_element_str = 'true'">
										<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_ToBeUpdated']"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$localeXml/translation[@id='TableHeaderText_MetadataElementsInReferenceStructure']"/>
									</xsl:otherwise>
								</xsl:choose>
							</th>
							<th />
						</tr>
						<tr>

							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_LocalName']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_LocalID']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_LocalAliases']"/>
							</th>

							<th/>

							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_IncomingName']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_IncomingID']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_IncomingAliases']"/>
							</th>
							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_RequestedByFeature']"/>
							</th>
						</tr>
					</xsl:when>

					<!-- Mode: Unmapped. Print incoming data columns. -->
					<xsl:when test="$mode = 'mode_unmapped'">
						<tr class="divider_column">
							<th class="highlightedheader">
								<xsl:attribute name="colspan">
									<xsl:value-of select="$colspanfull"/>
								</xsl:attribute>
								<xsl:choose>
									<xsl:when test="$is_full_element_str = 'true'">
										<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_ToBeAdded']"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_UnsuccessfulDeps']"/>
									</xsl:otherwise>
								</xsl:choose>
							</th>
						</tr>
						<tr class="divider_column border_left">
							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Name']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_ID']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Aliases']"/>
							</th>
							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_RequestedByFeature']"/>
							</th>
						</tr>
					</xsl:when>

					<!-- Mode: Skipped. Print incoming data columns. -->
					<xsl:when test="$mode = 'mode_skipped'">
						<tr class="divider_column">
							<th class="highlightedheader">
								<xsl:attribute name="colspan">
									<xsl:value-of select="$colspanfull"/>
								</xsl:attribute>
								<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_ToBeSkipped']"/>
							</th>
						</tr>
						<tr class="divider_column border_left">
							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Name']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_ID']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Aliases']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Reason']"/>
							</th>
						</tr>
					</xsl:when>

					<!-- Mode: Unchanged. Print incoming data columns and local id. -->
					<xsl:when test="$mode = 'mode_unchanged'">
						<tr class="divider_column">
							<th class="highlightedheader">
								<xsl:attribute name="colspan">
									<xsl:value-of select="$colspanfull"/>
								</xsl:attribute>
								<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_Unchanged']"/>
							</th>
						</tr>
						<tr class="divider_column border_left">
							<th class="border_left">
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Name']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_ID']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_LocalID2']"/>
							</th>
							<th>
								<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Aliases']"/>
							</th>
						</tr>
					</xsl:when>
					<xsl:otherwise>
					</xsl:otherwise>
				</xsl:choose>

				<!-- Print table data -->

				<!-- User accounts -->
				<xsl:variable name="subset_useraccounts" select="$path_to_table_element/useraccounts/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_useraccounts) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_UserAccounts']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_useraccounts"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- User groups -->
				<xsl:variable name="subset_usergroups" select="$path_to_table_element/usergroups/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_usergroups) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_UserGroups']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_usergroups"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Object Types -->
				<xsl:variable name="subset_objecttypes" select="$path_to_table_element/objecttypes/incoming[(@realobj='true') and (@full=$is_full_element_str) and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_objecttypes) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ObjectTypes']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_objecttypes"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Value Lists -->
				<xsl:variable name="subset_valuelists" select="$path_to_table_element/objecttypes/incoming[(@realobj='false') and (@full=$is_full_element_str) and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_valuelists) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ValueLists']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_valuelists"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Property Definitions -->
				<xsl:variable name="subset_propertydefs" select="$path_to_table_element/propertydefs/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_propertydefs) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_PropertyDefs']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_propertydefs"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Classes -->
				<xsl:variable name="subset_classes" select="$path_to_table_element/classes/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_classes) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_Classes']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_classes"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Class groups -->
				<xsl:variable name="subset_classgroups" select="$path_to_table_element/classgroups/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_classgroups) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ClassGroups']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_classgroups"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Workflows -->
				<xsl:variable name="subset_workflows" select="$path_to_table_element/workflows/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_workflows) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_Workflows']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_workflows"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Workflow states -->
				<xsl:variable name="subset_states" select="$path_to_table_element/states/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_states) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_WorkflowStates']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_states"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- NACLs -->
				<xsl:variable name="subset_namedacls" select="$path_to_table_element/namedacls/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_namedacls) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_NACLs']"/>
						</td>
					</tr>
					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_namedacls"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="nameattr" select="'name'"/>
						<xsl:with-param name="aliasesattr" select="'aliases'"/>
					</xsl:call-template>
				</xsl:if>

				<!-- View defs -->
				<xsl:variable name="subset_viewdefs" select="$path_to_table_element/viewdefs/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_viewdefs) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ViewDefs']"/>
						</td>
					</tr>

					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_viewdefs"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="alias_as_name" select="'true'"/>
						<xsl:with-param name="nameattr" select="'aliases'"/>
						<!-- Use aliases instead of name -->
						<xsl:with-param name="aliasesattr" select="''"/>
					</xsl:call-template>
				</xsl:if>

				<!-- Event handlers -->
				<xsl:variable name="subset_eventhandlers" select="$path_to_table_element/eventhandlers/incoming[@full=$is_full_element_str and not(@nochange=$is_changed_str) and boolean(ref-by-selectors/selector)]"/>
				<xsl:if test="count($subset_eventhandlers) &gt; 0">
					<tr class="table_subtitle">
						<td>
							<xsl:attribute name="colspan">
								<xsl:value-of select="$colspanfull"/>
							</xsl:attribute>
							<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_EventHandlers']"/>
						</td>
					</tr>

					<xsl:call-template name="print_rows_local_and_incoming">
						<xsl:with-param name="path_to_element" select="$subset_eventhandlers"/>
						<xsl:with-param name="mode" select="$mode"/>
						<xsl:with-param name="alias_as_name" select="'true'"/>
						<xsl:with-param name="nameattr" select="'aliases'"/>
						<!-- Use aliases instead of name -->
						<xsl:with-param name="aliasesattr" select="''"/>
					</xsl:call-template>
				</xsl:if>

			</table>

		</xsl:if>

	</xsl:template>

	<!-- *** Template: print_rows_local_and_incoming -->

	<xsl:template name="print_rows_local_and_incoming">
		<xsl:param name="path_to_element"/>
		<xsl:param name="mode"/>
		<xsl:param name="nameattr"/>
		<xsl:param name="aliasesattr"/>

		<xsl:for-each select="$path_to_element">

			<!-- Sort by name. -->
			<xsl:sort select="@*[name() = $nameattr]" />

			<tr>
				<!-- Change background color for every other row. -->
				<xsl:if test="(position() mod 2 = 0)">
					<xsl:attribute name="class">even_row</xsl:attribute>
				</xsl:if>

				<xsl:choose>

					<!-- Mode: Mapped. Print local and incoming data columns. -->
					<xsl:when test="$mode = 'mode_mapped'">
						<xsl:call-template name="print_rows_mapped">
							<xsl:with-param name="nameattr" select="$nameattr"/>
							<xsl:with-param name="aliasesattr" select="$aliasesattr"/>
						</xsl:call-template>
					</xsl:when>

					<!-- Mode: Unmapped. Print incoming data columns. -->
					<xsl:when test="$mode = 'mode_unmapped'">
						<td>
							<xsl:call-template name="prepare_clickable_element">
								<xsl:with-param name="mode" select="$mode"/>
							</xsl:call-template>

							<xsl:value-of select="@*[name() = $nameattr]"/>
							<xsl:if test="@deleted = 'true'" >
								<xsl:text> </xsl:text>
								<i>
									<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
								</i>
							</xsl:if>
						</td>
						<td>
							<xsl:value-of select="@id"/>
						</td>
						<td>
							<xsl:value-of select="@*[name() = $aliasesattr]"/>
						</td>
						<td>
							<xsl:variable name="referringSelectors" select="ref-by-selectors/selector" />
							<xsl:for-each select="callbackObj:GetModuleConfNamesFromSelectors($referringSelectors)/module-confs/module-conf">
								<xsl:value-of select="."/>
								<xsl:if test="not(position() = last())">
									<br/>
								</xsl:if>
							</xsl:for-each>
						</td>
					</xsl:when>

					<!-- Mode: Skipped. Print incoming data columns and the reason. -->
					<xsl:when test="$mode = 'mode_skipped'">
						<td>
							<xsl:call-template name="prepare_clickable_element">
								<xsl:with-param name="mode" select="$mode"/>
							</xsl:call-template>

							<xsl:value-of select="@*[name() = $nameattr]"/>
							<xsl:if test="@deleted = 'true'" >
								<xsl:text> </xsl:text>
								<i>
									<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
								</i>
							</xsl:if>
						</td>
						<td>
							<xsl:value-of select="@id"/>
						</td>
						<td>
							<xsl:value-of select="@*[name() = $aliasesattr]"/>
						</td>
						<td>
							<xsl:value-of select="reason/@value"/>
						</td>
					</xsl:when>

					<!-- Mode: Unchanged. Print incoming data columns. -->
					<xsl:when test="$mode = 'mode_unchanged'">
						<td>
							<xsl:call-template name="prepare_clickable_element">
								<xsl:with-param name="mode" select="$mode"/>
							</xsl:call-template>

							<xsl:value-of select="@*[name() = $nameattr]"/>
							<xsl:if test="@deleted = 'true'" >
								<xsl:text> </xsl:text>
								<i>
									<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
								</i>
							</xsl:if>
						</td>
						<td>
							<xsl:value-of select="@id"/>
						</td>
						<td>
							<xsl:value-of select="local/@id"/>
						</td>
						<td>
							<xsl:value-of select="@*[name() = $aliasesattr]"/>
						</td>
					</xsl:when>
				</xsl:choose>
			</tr>

		</xsl:for-each>
	</xsl:template>


	<!-- *** Template: print_rows_mapped -->

	<xsl:template name="print_rows_mapped">
		<xsl:param name="nameattr"/>
		<xsl:param name="aliasesattr"/>

		<xsl:call-template name="print_cell_local_and_incoming">
			<xsl:with-param name="local" select="local/@*[name() = $nameattr]"/>
			<xsl:with-param name="incoming" select="@*[name() = $nameattr]"/>
			<xsl:with-param name="output" select="local/@*[name() = $nameattr]"/>
			<xsl:with-param name="deleted" select="local/@deleted = 'true'"/>
			<xsl:with-param name="showXML" select="'true'"/>
		</xsl:call-template>
		<xsl:call-template name="print_cell_local_and_incoming">
			<!-- If no highligh wanted, the local and incoming parameters should be the same. -->
			<xsl:with-param name="local" select="local/@id"/>
			<xsl:with-param name="incoming" select="local/@id"/>
			<xsl:with-param name="output" select="local/@id"/>
			<xsl:with-param name="deleted" select="false()"/>
			<xsl:with-param name="showXML" select="'false'"/>
		</xsl:call-template>
		<xsl:call-template name="print_cell_local_and_incoming">
			<xsl:with-param name="local" select="local/@*[name() = $aliasesattr]"/>
			<xsl:with-param name="incoming" select="@*[name() = $aliasesattr]"/>
			<xsl:with-param name="output" select="local/@*[name() = $aliasesattr]"/>
			<xsl:with-param name="deleted" select="false()"/>
			<xsl:with-param name="showXML" select="'false'"/>
		</xsl:call-template>

		<td class="divider_column" ></td>

		<xsl:call-template name="print_cell_local_and_incoming">
			<xsl:with-param name="local" select="local/@*[name() = $nameattr]"/>
			<xsl:with-param name="incoming" select="@*[name() = $nameattr]"/>
			<xsl:with-param name="output" select="@*[name() = $nameattr]"/>
			<xsl:with-param name="deleted" select="@deleted = 'true'"/>
			<xsl:with-param name="showXML" select="'true'"/>
		</xsl:call-template>
		<xsl:call-template name="print_cell_local_and_incoming">
			<!-- If no highligh wanted, the local and incoming parameters should be the same. -->
			<xsl:with-param name="local" select="@id"/>
			<xsl:with-param name="incoming" select="@id"/>
			<xsl:with-param name="output" select="@id"/>
			<xsl:with-param name="deleted" select="false()"/>
			<xsl:with-param name="showXML" select="'false'"/>
		</xsl:call-template>
		<xsl:call-template name="print_cell_local_and_incoming">
			<xsl:with-param name="local" select="local/@*[name() = $aliasesattr]"/>
			<xsl:with-param name="incoming" select="@*[name() = $aliasesattr]"/>
			<xsl:with-param name="output" select="@*[name() = $aliasesattr]"/>
			<xsl:with-param name="deleted" select="false()"/>
			<xsl:with-param name="showXML" select="'false'"/>
		</xsl:call-template>
		<td>
			<xsl:variable name="referringSelectors" select="ref-by-selectors/selector" />
			<xsl:for-each select="callbackObj:GetModuleConfNamesFromSelectors($referringSelectors)/module-confs/module-conf">
				<xsl:value-of select="." />
				<xsl:if test="not(position() = last())">
					<br/>
				</xsl:if>
			</xsl:for-each>
		</td>

	</xsl:template>



	<!-- *** Template: print_cell_local_and_incoming -->

	<xsl:template name="print_cell_local_and_incoming">
		<xsl:param name="local"/>
		<xsl:param name="incoming"/>
		<xsl:param name="output"/>
		<xsl:param name="deleted"/>
		<xsl:param name="showXML"/>
		<xsl:choose>
			<xsl:when test="$local and $incoming">
				<xsl:choose>
					<xsl:when test="$local=$incoming">
						<td class="samevalue" >
							<xsl:if test="$showXML='true'">
								<xsl:call-template name="prepare_clickable_element">
									<xsl:with-param name="mode" select="'mode_mapped'"/>
								</xsl:call-template>
							</xsl:if>

							<xsl:value-of select="$output"/>
							<xsl:if test="$deleted" >
								<xsl:text> </xsl:text>
								<i>
									<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
								</i>
							</xsl:if>
						</td>
					</xsl:when>
					<xsl:otherwise>
						<td>
							<xsl:if test="$showXML='true'">
								<xsl:call-template name="prepare_clickable_element">
									<xsl:with-param name="mode" select="'mode_mapped'"/>
								</xsl:call-template>
							</xsl:if>

							<span class="differentvalue_text">
								<xsl:value-of select="$output"/>
							</span>
							<xsl:if test="$deleted" >
								<xsl:text> </xsl:text>
								<i>
									<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
								</i>
							</xsl:if>
						</td>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<td class="samevalue" >
					<xsl:if test="$showXML='true'">
						<xsl:call-template name="prepare_clickable_element">
							<xsl:with-param name="mode" select="''"/>
						</xsl:call-template>
					</xsl:if>

					<xsl:value-of select="$output"/>
					<xsl:if test="$deleted" >
						<xsl:text> </xsl:text>
						<i>
							<xsl:value-of select="$localeXml/translation[@id='CellContentText_Deleted']"/>
						</i>
					</xsl:if>
				</td>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- *** Template: print_table_unfound -->

	<xsl:template name="print_table_unfound">

		<table class="details_table">
			<tr>
				<th class="highlightedheader">
					<xsl:attribute name="colspan">
						<xsl:value-of select="$colspanfull"/>
					</xsl:attribute>
					<xsl:value-of select="$localeXml/translation[@id='TableHeaderText_UnfoundMetadataElements']"/>
				</th>
			</tr>
			<tr>
				<th>
					<xsl:value-of select="$localeXml/translation[@id='TableColNameText_Identifier']"/>
				</th>
				<th>
					<xsl:value-of select="$localeXml/translation[@id='TableColNameText_RequestedByFeature']"/>
				</th>
			</tr>

			<!-- Print table data -->

			<!-- User accounts -->
			<xsl:variable name="subset_useraccounts_all" select="useraccounts/selector"/>
			<xsl:variable name="subset_useraccounts" select="callbackObj:GetMissingSelectorsFromSelectors($subset_useraccounts_all)/selectors/selector" />
			<xsl:if test="count($subset_useraccounts) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_UserAccounts']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_useraccounts"/>
				</xsl:call-template>
			</xsl:if>

			<!-- User groups -->
			<xsl:variable name="subset_usergroups_all" select="usergroups/selector"/>
			<xsl:variable name="subset_usergroups" select="callbackObj:GetMissingSelectorsFromSelectors($subset_usergroups_all)/selectors/selector" />
			<xsl:if test="count($subset_usergroups) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_UserGroups']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_usergroups"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Object Types and Value List Items-->
			<xsl:variable name="subset_objecttypes_all" select="objecttypes/selector"/>
			<xsl:variable name="subset_objecttypes" select="callbackObj:GetMissingSelectorsFromSelectors($subset_objecttypes_all)/selectors/selector" />
			<xsl:if test="count($subset_objecttypes) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='StructureSubTitleText_ObjectTypesAndValuelists']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_objecttypes"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Property Definitions -->
			<xsl:variable name="subset_propertydefs_all" select="propertydefs/selector"/>
			<xsl:variable name="subset_propertydefs" select="callbackObj:GetMissingSelectorsFromSelectors($subset_propertydefs_all)/selectors/selector" />
			<xsl:if test="count($subset_propertydefs) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_PropertyDefs']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_propertydefs"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Classes -->
			<xsl:variable name="subset_classes_all" select="classes/selector"/>
			<xsl:variable name="subset_classes" select="callbackObj:GetMissingSelectorsFromSelectors($subset_classes_all)/selectors/selector" />
			<xsl:if test="count($subset_classes) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_Classes']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_classes"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Class groups -->
			<xsl:variable name="subset_classgroups_all" select="classgroups/selector"/>
			<xsl:variable name="subset_classgroups" select="callbackObj:GetMissingSelectorsFromSelectors($subset_classgroups_all)/selectors/selector" />
			<xsl:if test="count($subset_classgroups) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ClassGroups']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_classgroups"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Workflows -->
			<xsl:variable name="subset_workflows_all" select="workflows/selector"/>
			<xsl:variable name="subset_workflows" select="callbackObj:GetMissingSelectorsFromSelectors($subset_workflows_all)/selectors/selector" />
			<xsl:if test="count($subset_workflows) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_Workflows']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_workflows"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Workflow states -->
			<xsl:variable name="subset_states_all" select="states/selector"/>
			<xsl:variable name="subset_states" select="callbackObj:GetMissingSelectorsFromSelectors($subset_states_all)/selectors/selector" />
			<xsl:if test="count($subset_states) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_WorkflowStates']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_states"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- NACLs -->
			<xsl:variable name="subset_namedacls_all" select="namedacls/selector"/>
			<xsl:variable name="subset_namedacls" select="callbackObj:GetMissingSelectorsFromSelectors($subset_namedacls_all)/selectors/selector" />
			<xsl:if test="count($subset_namedacls) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_NACLs']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_namedacls"/>
					<xsl:with-param name="aliasesattr" select="'aliases'"/>
				</xsl:call-template>
			</xsl:if>

			<!-- View defs -->
			<xsl:variable name="subset_viewdefs_all" select="viewdefs/selector"/>
			<xsl:variable name="subset_viewdefs" select="callbackObj:GetMissingSelectorsFromSelectors($subset_viewdefs_all)/selectors/selector" />
			<xsl:if test="count($subset_viewdefs) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_ViewDefs']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_viewdefs"/>
				</xsl:call-template>
			</xsl:if>

			<!-- Event handlers -->
			<xsl:variable name="subset_eventhandlers_all" select="eventhandlers/selector"/>
			<xsl:variable name="subset_eventhandlers" select="callbackObj:GetMissingSelectorsFromSelectors($subset_eventhandlers_all)/selectors/selector" />
			<xsl:if test="count($subset_eventhandlers) &gt; 0">
				<tr class="table_subtitle">
					<td>
						<xsl:attribute name="colspan">
							<xsl:value-of select="$colspanfull"/>
						</xsl:attribute>
						<xsl:value-of select="$localeXml/translation[@id='TableSubTitleText_EventHandlers']"/>
					</td>
				</tr>
				<xsl:call-template name="print_rows_unfound">
					<xsl:with-param name="path_to_element" select="$subset_eventhandlers"/>
				</xsl:call-template>
			</xsl:if>

		</table>

	</xsl:template>


	<!-- *** Template: print_rows_unfound -->

	<xsl:template name="print_rows_unfound">
		<xsl:param name="path_to_element"/>
		<xsl:for-each select="$path_to_element">

			<tr>
				<!-- Change background color for every other row. -->
				<xsl:if test="(position() mod 2 = 0)">
					<xsl:attribute name="class">even_row</xsl:attribute>
				</xsl:if>
				<xsl:variable name="selectorId" select="." />
				<td>
					<xsl:value-of select="callbackObj:GetSelectorReference($selectorId)"/>
				</td>
				<td>
					<xsl:value-of select="callbackObj:GetSelectorModuleConf($selectorId)"/>
				</td>
			</tr>
		</xsl:for-each>
	</xsl:template>



	<!-- *** Template: print_rows_unfound -->

	<xsl:template name="prepare_clickable_element">
		<xsl:param name="mode"/>

		<xsl:if test="@canbefull = 'true'">
			<xsl:attribute name="onClick">
				<xsl:text disable-output-escaping="yes">parent.parent.ShowXML( '</xsl:text>
				<xsl:if test="$mode = 'mode_mapped'">
					<xsl:value-of select="callbackObj:GetXMLFromLocalVault(local/@guid)"/>
					<xsl:text disable-output-escaping="yes">', '</xsl:text>
				</xsl:if>
				<xsl:value-of select="callbackObj:GetXMLFromRemoteVault(@guid)"/>
				<xsl:text disable-output-escaping="yes">' );</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="class">
				clickable
				<xsl:if test="(position() mod 2 = 0)">
					even_row
				</xsl:if>
			</xsl:attribute>
		</xsl:if>

	</xsl:template>



	<!-- *** MAIN: replicationReport -->

	<xsl:template match="/replicationReport">

		<!-- Title. -->
		<h1>
			<xsl:value-of select="$localeXml/translation[@id='MainTitleText_UpgradeVaultStructure']"/>
		</h1>

		<!-- Structure section details. -->
		<xsl:apply-templates select="/replicationReport/structure"/>
		<xsl:apply-templates select="/replicationReport/unfound-selectors"/>

		<hr/>
		<small>
			<em>
				<xsl:value-of select="$localeXml/translation[@id='MainFooterText']"/>
			</em>
		</small>

	</xsl:template>


	<!-- *** Template: Handle metadata structure data -->

	<xsl:template match="/replicationReport/structure">

		<!-- Unmapped. -->
		<xsl:if test="count(unmapped//incoming[@full='true']) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_NewMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_NewMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="unmapped"/>
				<xsl:with-param name="mode" select="'mode_unmapped'"/>
				<xsl:with-param name="is_full_element_str" select="'true'"/>
				<xsl:with-param name="is_changed_str" select="'true'"/>
			</xsl:call-template>
		</xsl:if>

		<!-- Mapped. -->
		<xsl:if test="count(mapped//incoming[@full='true' and not(@nochange='true')]) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_ExistingMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_ExistingMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="mapped"/>
				<xsl:with-param name="mode" select="'mode_mapped'"/>
				<xsl:with-param name="is_full_element_str" select="'true'"/>
				<xsl:with-param name="is_changed_str" select="'true'"/>
			</xsl:call-template>
		</xsl:if>

		<!-- Skipped. -->
		<xsl:if test="count(skipped//incoming[@full='true']) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_SkippedMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_SkippedMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="skipped"/>
				<xsl:with-param name="mode" select="'mode_skipped'"/>
				<xsl:with-param name="is_full_element_str" select="'true'"/>
				<xsl:with-param name="is_changed_str" select="'true'"/>
			</xsl:call-template>
		</xsl:if>

		<!-- Unchanged. -->
		<xsl:if test="count(mapped//incoming[@full='true' and @nochange='true']) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_UnchangedMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_UnchangedMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="mapped"/>
				<xsl:with-param name="mode" select="'mode_unchanged'"/>
				<xsl:with-param name="is_full_element_str" select="'true'"/>
				<xsl:with-param name="is_changed_str" select="'false'"/>
			</xsl:call-template>
		</xsl:if>

		<!-- Successful dependencies. -->
		<xsl:if test="count(mapped//incoming[@full='false' and boolean(ref-by-selectors/selector)]) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_DependenciesToExistingMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_DependenciesToExistingMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="mapped"/>
				<xsl:with-param name="mode" select="'mode_mapped'"/>
				<xsl:with-param name="is_full_element_str" select="'false'"/>
				<xsl:with-param name="is_changed_str" select="'true'"/>
			</xsl:call-template>
		</xsl:if>

		<!-- Unsuccessful dependencies. -->
		<xsl:if test="count(unmapped//incoming[@full='false' and boolean(ref-by-selectors/selector)]) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_DependenciesToMissingMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_DependenciesToMissingMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_local_and_incoming">
				<xsl:with-param name="path_to_table_element" select="unmapped"/>
				<xsl:with-param name="mode" select="'mode_unmapped'"/>
				<xsl:with-param name="is_full_element_str" select="'false'"/>
				<xsl:with-param name="is_changed_str" select="'true'"/>
			</xsl:call-template>
		</xsl:if>

	</xsl:template>

	<!-- Selected but unfound. -->
	<xsl:template match="/replicationReport/unfound-selectors">

		<xsl:variable name="selectorIDs" select=".//selector" />
		<xsl:variable name="missingSelectorIDs" select="callbackObj:GetMissingSelectorsFromSelectors($selectorIDs)/selectors/selector" />
		<xsl:if test="count($missingSelectorIDs) &gt; 0">
			<h2><xsl:value-of select="$localeXml/translation[@id='MainSectionHeader_NonExistingMetadataStructureElements']"/></h2>
			<p>
				<xsl:value-of select="$localeXml/translation[@id='MainSectionText_NonExistingMetadataStructureElements']"/>
			</p>
			<xsl:call-template name="print_table_unfound">
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>


