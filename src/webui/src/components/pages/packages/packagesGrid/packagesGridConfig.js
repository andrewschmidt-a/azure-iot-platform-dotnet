// Copyright (c) Microsoft. All rights reserved.

import Config from 'app.config';
import { IsActivePackageRenderer, TimeRenderer, SoftSelectLinkRenderer } from 'components/shared/cellRenderers';
import { EMPTY_FIELD_VAL, gridValueFormatters } from 'components/shared/pcsGrid/pcsGridConfig';
import { getPackageTypeTranslation, getConfigTypeTranslation, svgs } from 'utilities';
import { INACTIVE_PACKAGE_TAG } from 'services/configService';

const { checkForEmpty } = gridValueFormatters;

export const packagesColumnDefs = {
  name: {
    headerName: 'packages.grid.name',
    field: 'name',
    sort: 'asc',
    valueFormatter: ({ value }) => checkForEmpty(value),
    cellRendererFramework: SoftSelectLinkRenderer
  },
  packageType: {
    headerName: 'packages.grid.packageType',
    field: 'packageType',
    valueFormatter: ({ value, context: { t } }) => getPackageTypeTranslation(checkForEmpty(value), t)
  },
  configType: {
    headerName: 'packages.grid.configType',
    field: 'configType',
    valueFormatter: ({ value, context: { t } }) => getConfigTypeTranslation(checkForEmpty(value), t)
  },
  dateCreated: {
    headerName: 'packages.grid.dateCreated',
    field: 'dateCreated',
    cellRendererFramework: TimeRenderer
  },
  active: {
    headerName: 'packages.grid.active',
    field: 'active',
    cellRendererFramework: IsActivePackageRenderer
  },
  tags: {
    headerName: 'packages.grid.tags',
    field: 'tags',
    valueFormatter: ({ value }) => Object.keys(value || {}).filter(key => key != INACTIVE_PACKAGE_TAG).join('; ') || EMPTY_FIELD_VAL
  },
};

export const defaultPackagesGridProps = {
  enableColResize: true,
  multiSelect: true,
  pagination: true,
  paginationPageSize: Config.paginationPageSize,
  rowSelection: 'multiple'
};
