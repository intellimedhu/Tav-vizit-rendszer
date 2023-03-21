<template>
    <div class="col pr-md-3 pr-lg-0 col-statistics">
        <div class="statistics-title">
            {{ title }}
        </div>

        <div class="inner border border-dark rounded">
            <div class="stat-block" v-for="block in blocks" :key="block.id">
                <slot :name="`block-header-${block.id}`" />
                <div class="row" v-for="(row, i) in block.rows" :key="`row-${block.id}-${i}`">
                    <div class="col-8 col-sm-9">
                        {{ row.label }}
                    </div>
                    <div class="col">
                        {{ row.dataText }}
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
export default {
  props: ["title", "blocks"]
};
</script>
