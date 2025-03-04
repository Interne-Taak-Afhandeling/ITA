{{- define "InterneTaakAfhandeling.Poller.fullname" -}}
{{- printf "%s-%s" .Release.Name "Poller" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "InterneTaakAfhandeling.Poller.appsettings" -}}
{{ toYaml .Values.appsettings | nindent 2 }}
{{- end }}
