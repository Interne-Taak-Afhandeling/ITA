{{- define "InterneTaakAfhandeling.Web.fullname" -}}
{{- printf "%s-%s" .Release.Name "Web" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "InterneTaakAfhandeling.Web.name" -}}
{{- printf "%s-Web" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}