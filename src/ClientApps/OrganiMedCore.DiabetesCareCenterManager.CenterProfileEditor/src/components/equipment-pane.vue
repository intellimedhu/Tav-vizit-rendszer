<template>
    <div class="card card-center-profile-editor card-center-profile-editor-primary">
        <div class="card-header">
            <h4 class="m-0">
                {{ headerText }}
            </h4>
        </div>
        
        <div class="card-body">            
            <div class="row">
                <div class="col-12 col-md-10 offset-md-2 col-lg-6 offset-lg-0"
                     :class="{'mb-3': equipmentIndex < equipments.length - 2}"
                     v-for="(equipment, equipmentIndex) in equipments"
                     v-bind:key="equipment.name">
                    <div class="row">
                        <label v-if="equipment.type != 1" class="col-12 col-sm col-form-label text-md-right" :for="equipment.id">
                            <span :class="{'font-weight-bold': equipment.required}">
                                {{ equipment.caption }}
                            </span>
                        </label>

                        <div class="col" :class="{'col-sm-equipment-tools': equipment.type != 1}">
                            <input type="number"
                                   v-if="equipment.type == 0"
                                   min="0"
                                   class="form-control"
                                   :id="equipment.id"
                                   v-model="equipment.value" />
                            <div class="custom-control custom-switch" v-if="equipment.type == 1">
                                <input type="checkbox"
                                       :id="equipment.id"
                                       class="custom-control-input"
                                       v-model="equipment.value" />
                                <label :for="equipment.id" class="custom-control-label">
                                    <span :class="{'font-weight-bold': equipment.required}">
                                        {{ equipment.caption }}
                                    </span>
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
  props: ["equipments", "type", "headerText"]
};
</script>


