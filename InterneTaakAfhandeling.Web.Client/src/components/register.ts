import type { App } from "vue";

import {
  PageHeader as UtrechtPageHeader,
  PageFooter as UtrechtPageFooter,
  Link as UtrechtLink,
  SkipLink as UtrechtSkipLink,
  Article as UtrechtArticle,
  Heading as UtrechtHeading,
  Paragraph as UtrechtParagraph,
  UnorderedList as UtrechtUnorderedList,
  UnorderedListItem as UtrechtUnorderedListItem,
  Button as UtrechtButton,
  FormField as UtrechtFormField,
  FormLabel as UtrechtFormLabel,
  Textbox as UtrechtTextbox,
  Table as UtrechtTable,
  TableCaption as UtrechtTableCaption,
  TableHeader as UtrechtTableHeader,
  TableBody as UtrechtTableBody,
  TableFooter as UtrechtTableFooter,
  TableRow as UtrechtTableRow,
  TableCell as UtrechtTableCell,
  TableHeaderCell as UtrechtTableHeaderCell,
  Select as UtrechtSelect,
  FormFieldset as UtrechtFieldset,
  FormFieldsetLegend as UtrechtLegend,
  Document as UtrechtDocument,
  DataList as UtrechtDataList,
  DataListItem as UtrechtDataListItem,
  DataListKey as UtrechtDataListKey,
  Textarea as UtrechtTextarea,
  RadioButton as UtrechtRadiobutton,
  AlertDialog as UtrechtAlertDialog,
  ButtonGroup as UtrechtButtonGroup
} from "@utrecht/component-library-vue";
// WORKAROUND, there's a bug in the original component
import UtrechtDataListValue from "./UtrechtDataListValue.vue";

const components = {
  UtrechtPageHeader,
  UtrechtPageFooter,
  UtrechtLink,
  UtrechtSkipLink,
  UtrechtArticle,
  UtrechtHeading,
  UtrechtParagraph,
  UtrechtUnorderedList,
  UtrechtUnorderedListItem,
  UtrechtButton,
  UtrechtFormField,
  UtrechtFormLabel,
  UtrechtTextbox,
  UtrechtTable,
  UtrechtTableCaption,
  UtrechtTableHeader,
  UtrechtTableBody,
  UtrechtTableFooter,
  UtrechtTableRow,
  UtrechtTableCell,
  UtrechtTableHeaderCell,
  UtrechtSelect,
  UtrechtFieldset,
  UtrechtLegend,
  UtrechtDocument,
  UtrechtDataList,
  UtrechtDataListItem,
  UtrechtDataListKey,
  UtrechtDataListValue,
  UtrechtTextarea,
  UtrechtRadiobutton,
  UtrechtAlertDialog,
  UtrechtButtonGroup
} as const;

export type OurComponents = typeof components;

export const registerComponents = (app: App): void =>
  Object.entries(components).forEach(([key, value]) => app.component(key, value));
