{{/* Common names and labels */}}
{{- define "internetaakafhandeling.name" -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "internetaakafhandeling.fullname" -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Web specific names */}}
{{- define "internetaakafhandeling.web.name" -}}
{{- printf "%s-web" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "internetaakafhandeling.web.fullname" -}}
{{- printf "%s-web" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Poller specific names */}}
{{- define "internetaakafhandeling.poller.name" -}}
{{- printf "%s-poller" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "internetaakafhandeling.poller.fullname" -}}
{{- printf "%s-poller" .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/* Common labels */}}
{{- define "internetaakafhandeling.labels" -}}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end -}}

{{/* Component labels */}}
{{- define "internetaakafhandeling.web.labels" -}}
{{ include "internetaakafhandeling.labels" . }}
app.kubernetes.io/name: web
app.kubernetes.io/component: web
{{- end -}}

{{- define "internetaakafhandeling.poller.labels" -}}
{{ include "internetaakafhandeling.labels" . }}
app.kubernetes.io/name: poller
app.kubernetes.io/component: poller
{{- end -}}

{{/* Selector labels */}}
{{- define "internetaakafhandeling.web.selectorLabels" -}}
app.kubernetes.io/name: {{ include "internetaakafhandeling.web.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: web
{{- end -}}

{{- define "internetaakafhandeling.poller.selectorLabels" -}}
app.kubernetes.io/name: {{ include "internetaakafhandeling.poller.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: poller
{{- end -}}

{{/* Database connection string helper for web */}}
{{- define "internetaakafhandeling.web.databaseConnectionString" -}}
{{- if .Values.postgresql.enabled -}}
  {{- printf "Host=%s-postgresql;Port=%s;Database=%s;Username=%s;Password=%s;" .Release.Name .Values.web.database.port .Values.web.database.name .Values.web.database.username .Values.web.database.password -}}
{{- else -}}
  {{- printf "Host=%s;Port=%s;Database=%s;Username=%s;Password=%s;" .Values.web.database.host .Values.web.database.port .Values.web.database.name .Values.web.database.username .Values.web.database.password -}}
{{- end -}}
{{- end -}}

{{/* Database connection string helper for poller */}}
{{- define "internetaakafhandeling.poller.databaseConnectionString" -}}
{{- if .Values.postgresql.enabled -}}
  {{- printf "Host=%s-postgresql;Port=%s;Database=%s;Username=%s;Password=%s;" .Release.Name .Values.poller.database.port .Values.poller.database.name .Values.poller.database.username .Values.poller.database.password -}}
{{- else -}}
  {{- printf "Host=%s;Port=%s;Database=%s;Username=%s;Password=%s;" .Values.poller.database.host .Values.poller.database.port .Values.poller.database.name .Values.poller.database.username .Values.poller.database.password -}}
{{- end -}}
{{- end -}}