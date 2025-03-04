{{- define "InterneTaakAfhandeling.fullname" -}}
{{- printf "%s" .Release.Name -}}
{{- end -}}

{{- define "InterneTaakAfhandeling.Web.name" -}}
{{- printf "%s-Web" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}